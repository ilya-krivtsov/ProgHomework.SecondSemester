namespace Zipper;

using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;

/// <summary>
/// Provides methods and properties used to transform and reconstruct data streams by using the BWT algorithm.
/// </summary>
internal class BWTStream : Stream
{
    /// <summary>
    /// Smallest allowed block length.
    /// </summary>
    public const int MinBlockSize = 1024;

    /// <summary>
    /// Largest allowed block length.
    /// </summary>
    public const int MaxBlockSize = 16384;

    private const int DefaultBlockSize = (MinBlockSize + MaxBlockSize) / 2;

    private static readonly ArrayPool<byte> BlockPool = ArrayPool<byte>.Create();

    private readonly Stream stream;
    private readonly BWTMode mode;
    private readonly bool leaveOpen;

    private byte[]? block;
    private int blockPosition;
    private int blockSize;

    private bool disposed;

    /// <inheritdoc cref="BWTStream(Stream, int, BWTMode, bool)"/>
    public BWTStream(Stream stream, BWTMode mode, bool leaveOpen = false)
        : this(stream, DefaultBlockSize, mode, leaveOpen)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BWTStream"/> class.
    /// </summary>
    /// <param name="stream">The stream to which transformed data is written or from which data to reconstruct is read.</param>
    /// <param name="blockSize">The internal block size to use, should be between <see cref="MinBlockSize"/> and <see cref="MaxBlockSize"/>.</param>
    /// <param name="mode"><see cref="BWTMode"/> that determines whether to transform or reconstruct data.</param>
    /// <param name="leaveOpen">
    /// The value indicating whether <paramref name="stream"/> should be disposed along with this instance,
    /// if <paramref name="mode"/> is <see cref="BWTMode.Transform"/>.
    /// </param>
    /// <exception cref="ArgumentException"><paramref name="mode"/> is not <see cref="BWTMode.Transform"/> nor <see cref="BWTMode.Reconstruct"/>.</exception>
    /// <exception cref="IndexOutOfRangeException"><paramref name="blockSize"/> is out of range.</exception>
    public BWTStream(Stream stream, int blockSize, BWTMode mode, bool leaveOpen = false)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(blockSize, MinBlockSize);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(blockSize, MaxBlockSize);

        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentException($"Value was neither {BWTMode.Transform} nor {BWTMode.Reconstruct}", nameof(mode));
        }

        if (mode == BWTMode.Transform)
        {
            if (!stream.CanWrite)
            {
                throw new ArgumentException("Stream does not support writing", nameof(stream));
            }

            this.blockSize = blockSize;
        }
        else
        {
            if (!stream.CanRead)
            {
                throw new ArgumentException("Stream does not support reading", nameof(stream));
            }
        }

        this.stream = stream;
        this.mode = mode;
        this.leaveOpen = leaveOpen;
        blockPosition = 0;
        disposed = false;
    }

    /// <summary>
    /// Gets a value indicating whether the stream supports reading.
    /// </summary>
    /// <inheritdoc/>
    public override bool CanRead => mode == BWTMode.Reconstruct && stream.CanRead;

    /// <summary>
    /// Gets a value indicating whether the stream supports writing.
    /// </summary>
    /// <inheritdoc/>
    public override bool CanWrite => mode == BWTMode.Transform && stream.CanWrite;

    /// <summary>
    /// Gets a value indicating whether the stream supports seeking.
    /// </summary>
    /// <inheritdoc/>
    public override bool CanSeek => false;

    /// <summary>
    /// This property is not supported and always throws a <see cref="NotSupportedException"/>.
    /// </summary>
    /// <inheritdoc/>
    public override long Length => throw new NotSupportedException();

    /// <summary>
    /// This property is not supported and always throws a <see cref="NotSupportedException"/>.
    /// </summary>
    /// <inheritdoc/>
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    /// <summary>
    /// This method is not supported and always throws a <see cref="NotSupportedException"/>.
    /// </summary>
    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin)
        => throw new NotSupportedException();

    /// <summary>
    /// This method is not supported and always throws a <see cref="NotSupportedException"/>.
    /// </summary>
    /// <inheritdoc/>
    public override void SetLength(long value)
        => throw new NotSupportedException();

    /// <summary>
    /// Flushes the internal buffers.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Stream is disposed.</exception>
    public override void Flush()
    {
        EnsureNotClosed();
        if (mode == BWTMode.Transform)
        {
            if (block != null)
            {
                WriteBlock();
            }

            stream.Flush();
        }
    }

    /// <summary>
    /// Reads data from <paramref name="buffer"/>, transforms it and writes it to the underlying stream.
    /// </summary>
    /// <param name="buffer">Buffer that contains data to be transformed.</param>
    /// <param name="offset">How many bytes to skip before reading from <paramref name="buffer"/>.</param>
    /// <param name="count">How many bytes to read from <paramref name="buffer"/>.</param>
    /// <exception cref="InvalidOperationException">Stream is set to <see cref="BWTMode.Reconstruct"/> mode.</exception>
    /// <exception cref="ObjectDisposedException">Stream is disposed.</exception>
    public override void Write(byte[] buffer, int offset, int count)
        => Write(buffer.AsSpan(offset, count));

    /// <summary>
    /// Reads data from the underlying stream, reconstructs it and writes to <paramref name="buffer"/>.
    /// </summary>
    /// <param name="buffer">Buffer to write reconstructed data to.</param>
    /// <param name="offset">How many bytes to skip before writing to <paramref name="buffer"/>.</param>
    /// <param name="count">How many bytes to write to <paramref name="buffer"/>.</param>
    /// <returns>Count of read bytes, can be less than <paramref name="count"/>.</returns>
    /// <exception cref="EndOfStreamException">Unexpected end of stream.</exception>
    /// <exception cref="InvalidDataException">Invalid data stream.</exception>
    /// <exception cref="InvalidOperationException">Stream is set to <see cref="BWTMode.Transform"/> mode.</exception>
    /// <exception cref="ObjectDisposedException">Stream is disposed.</exception>
    public override int Read(byte[] buffer, int offset, int count)
        => Read(buffer.AsSpan(offset, count));

    /// <inheritdoc cref="Write(byte[], int, int)"/>
    public override void Write(ReadOnlySpan<byte> buffer)
    {
        EnsureNotClosed();
        EnsureMode(BWTMode.Transform);

        int bufferPosition = 0;
        while (bufferPosition < buffer.Length)
        {
            if (block == null)
            {
                block = BlockPool.Rent(blockSize);
                blockPosition = 0;
            }

            int copyLength = Math.Min(blockSize - blockPosition, buffer.Length - bufferPosition);
            buffer.Slice(bufferPosition, copyLength).CopyTo(block.AsSpan().Slice(blockPosition, copyLength));

            bufferPosition += copyLength;
            blockPosition += copyLength;

            if (blockPosition >= blockSize)
            {
                WriteBlock();
            }
        }
    }

    /// <inheritdoc cref="Read(byte[], int, int)"/>
    public override int Read(Span<byte> buffer)
    {
        EnsureNotClosed();
        EnsureMode(BWTMode.Reconstruct);

        int bufferPosition = 0;
        while (bufferPosition < buffer.Length)
        {
            if (block == null && !ReadBlock())
            {
                break;
            }

            int copyLength = Math.Min(blockSize - blockPosition, buffer.Length - bufferPosition);
            block.AsSpan().Slice(blockPosition, copyLength).CopyTo(buffer.Slice(bufferPosition, copyLength));

            bufferPosition += copyLength;
            blockPosition += copyLength;

            if (blockPosition >= blockSize)
            {
                block = null;
            }
        }

        return bufferPosition;
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            Flush();

            Debug.Assert(block == null, "Block was not returned");

            if (!leaveOpen)
            {
                stream.Dispose();
            }

            disposed = true;
        }
    }

    private void WriteBlock()
    {
        Debug.Assert(block != null, "Block is null");

        var transformBuffer = BlockPool.Rent(blockPosition);
        int identityIndex = BWT.ForwardTransform(block.AsSpan()[0..blockPosition], transformBuffer.AsSpan()[0..blockPosition]);

        Span<byte> header = stackalloc byte[8];
        BinaryPrimitives.WriteInt32LittleEndian(header[0..4], blockPosition);
        BinaryPrimitives.WriteInt32LittleEndian(header[4..8], identityIndex);

        stream.Write(header);
        stream.Write(transformBuffer, 0, blockPosition);

        BlockPool.Return(transformBuffer);
        BlockPool.Return(block);

        block = null;
    }

    private bool ReadBlock()
    {
        Debug.Assert(block == null, "Block was not returned before reading");

        Span<byte> header = stackalloc byte[8];
        if (stream.Read(header) != 8)
        {
            return false;
        }

        blockSize = BinaryPrimitives.ReadInt32LittleEndian(header[0..4]);
        var identityIndex = BinaryPrimitives.ReadInt32LittleEndian(header[4..8]);

        if (identityIndex < 0 || identityIndex >= blockSize)
        {
            throw new InvalidDataException();
        }

        var transformedData = BlockPool.Rent(blockSize);
        if (stream.Read(transformedData, 0, blockSize) != blockSize)
        {
            BlockPool.Return(transformedData);

            throw new EndOfStreamException();
        }

        block = BlockPool.Rent(blockSize);
        BWT.InverseTransform(transformedData.AsSpan()[0..blockSize], identityIndex, block.AsSpan()[0..blockSize]);
        BlockPool.Return(transformedData);
        blockPosition = 0;

        return true;
    }

    private void EnsureMode(BWTMode mode)
    {
        if (this.mode != mode)
        {
            throw new InvalidOperationException();
        }
    }

    private void EnsureNotClosed()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
    }
}
