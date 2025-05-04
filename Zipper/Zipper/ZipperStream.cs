namespace Zipper;

using Zipper.LZW;

/// <summary>
/// Provides methods and properties used to compress and decompress streams.
/// </summary>
public class ZipperStream : Stream
{
    /// <summary>
    /// Smallest allowed block length.
    /// </summary>
    public const int MinBlockSize = LZWStream.MinBlockSize;

    /// <summary>
    /// Largest allowed block length.
    /// </summary>
    public const int MaxBlockSize = LZWStream.MaxBlockSize;

    private const int DefaultBlockSize = (MinBlockSize + MaxBlockSize) / 2;

    private readonly LZWStream lzwStream;
    private readonly BWTStream bwtStream;

    private readonly Stream stream;
    private readonly ZipperMode mode;
    private readonly bool leaveOpen;

    private bool disposed;

    /// <inheritdoc cref="ZipperStream(Stream, int, ZipperMode, bool)"/>
    public ZipperStream(Stream stream, ZipperMode mode, bool leaveOpen = false)
        : this(stream, DefaultBlockSize, mode, leaveOpen)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ZipperStream"/> class.
    /// </summary>
    /// <param name="stream">The stream to which compressed data is written or from which data to decompress is read.</param>
    /// <param name="blockSize">The internal block size to use, should be between <see cref="MinBlockSize"/> and <see cref="MaxBlockSize"/>.</param>
    /// <param name="mode"><see cref="ZipperMode"/> that determines whether to compress or decompress data.</param>
    /// <param name="leaveOpen">
    /// The value indicating whether <paramref name="stream"/> should be disposed along with this instance,
    /// if <paramref name="mode"/> is <see cref="ZipperMode.Compress"/>.
    /// </param>
    /// <exception cref="ArgumentException"><paramref name="mode"/> is not <see cref="ZipperMode.Compress"/> nor <see cref="ZipperMode.Decompress"/>.</exception>
    /// <exception cref="IndexOutOfRangeException"><paramref name="blockSize"/> is out of range.</exception>
    public ZipperStream(Stream stream, int blockSize, ZipperMode mode, bool leaveOpen = false)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(blockSize, MinBlockSize);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(blockSize, MaxBlockSize);

        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentException($"Value was neither {ZipperMode.Compress} nor {ZipperMode.Decompress}", nameof(mode));
        }

        float relativeBlockSize = (blockSize - MinBlockSize) / (float)(MaxBlockSize - MinBlockSize);
        int bwtBlockSize = (int)(BWTStream.MinBlockSize + (relativeBlockSize * (BWTStream.MaxBlockSize - BWTStream.MinBlockSize)));

        this.stream = stream;

        lzwStream = new(stream, blockSize, mode, true);
        bwtStream = new(lzwStream, bwtBlockSize, mode == ZipperMode.Compress ? BWTMode.Transform : BWTMode.Reconstruct, true);

        this.mode = mode;
        this.leaveOpen = leaveOpen;
        disposed = false;
    }

    /// <summary>
    /// Gets a value indicating whether the stream supports reading.
    /// </summary>
    /// <inheritdoc/>
    public override bool CanRead => mode == ZipperMode.Decompress && stream.CanRead;

    /// <summary>
    /// Gets a value indicating whether the stream supports writing.
    /// </summary>
    /// <inheritdoc/>
    public override bool CanWrite => mode == ZipperMode.Compress && stream.CanWrite;

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

        if (mode == ZipperMode.Compress)
        {
            bwtStream.Flush();
        }
    }

    /// <summary>
    /// Reads data from <paramref name="buffer"/>, compresses it and writes it to the underlying stream.
    /// </summary>
    /// <param name="buffer">Buffer that contains data to be compressed.</param>
    /// <param name="offset">How many bytes to skip before reading from <paramref name="buffer"/>.</param>
    /// <param name="count">How many bytes to read from <paramref name="buffer"/>.</param>
    /// <exception cref="InvalidOperationException">Stream is set to <see cref="ZipperMode.Decompress"/> mode.</exception>
    /// <exception cref="ObjectDisposedException">Stream is disposed.</exception>
    public override void Write(byte[] buffer, int offset, int count)
        => Write(buffer.AsSpan(offset, count));

    /// <summary>
    /// Reads data from the underlying stream, decompresses it and writes to <paramref name="buffer"/>.
    /// </summary>
    /// <param name="buffer">Buffer to write decompressed data to.</param>
    /// <param name="offset">How many bytes to skip before writing to <paramref name="buffer"/>.</param>
    /// <param name="count">How many bytes to write to <paramref name="buffer"/>.</param>
    /// <returns>Count of read bytes, can be less than <paramref name="count"/>.</returns>
    /// <exception cref="EndOfStreamException">Unexpected end of stream.</exception>
    /// <exception cref="InvalidDataException">Invalid data stream.</exception>
    /// <exception cref="InvalidOperationException">Stream is set to <see cref="ZipperMode.Compress"/> mode.</exception>
    /// <exception cref="ObjectDisposedException">Stream is disposed.</exception>
    public override int Read(byte[] buffer, int offset, int count)
        => Read(buffer.AsSpan(offset, count));

    /// <inheritdoc cref="Write(byte[], int, int)"/>
    public override void Write(ReadOnlySpan<byte> buffer)
    {
        EnsureNotClosed();
        EnsureMode(ZipperMode.Compress);

        bwtStream.Write(buffer);
    }

    /// <inheritdoc cref="Read(byte[], int, int)"/>
    public override int Read(Span<byte> buffer)
    {
        EnsureNotClosed();
        EnsureMode(ZipperMode.Decompress);

        return bwtStream.Read(buffer);
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
            bwtStream.Dispose();
            lzwStream.Dispose();

            if (!leaveOpen)
            {
                stream.Dispose();
            }

            disposed = true;
        }
    }

    private void EnsureMode(ZipperMode mode)
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
