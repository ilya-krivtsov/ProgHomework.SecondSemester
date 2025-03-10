namespace Zipper.LZW;

using System.Diagnostics;

/// <summary>
/// Provides methods and properties used to compress and decompress streams by using the LZW algorithm.
/// </summary>
public class LZWStream : Stream
{
    /// <summary>
    /// Smallest allowed Block length.
    /// </summary>
    public const int MinBlockSize = 256;

    /// <summary>
    /// Largest allowed Block length.
    /// </summary>
    public const int MaxBlockSize = 64 * 1024;

    private const int DefaultBlockSize = 1024;

    private readonly Stream stream;
    private readonly ZipperMode mode;
    private readonly bool leaveOpen;

    private readonly LZWWriter? writer;
    private readonly LZWReader? reader;

    private bool disposed;

    /// <inheritdoc cref="LZWStream(Stream, int, ZipperMode, bool)"/>
    public LZWStream(Stream stream, ZipperMode mode, bool leaveOpen = false)
        : this(stream, DefaultBlockSize, mode, leaveOpen)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LZWStream"/> class.
    /// </summary>
    /// <param name="stream">The stream to which compressed data is written or from which data to uncompress is read.</param>
    /// <param name="blockSize">The internal block size to use, should be between <see cref="MinBlockSize"/> and <see cref="MaxBlockSize"/>.</param>
    /// <param name="mode"><see cref="ZipperMode"/> that determines whether to compress or uncompress data.</param>
    /// <param name="leaveOpen">
    /// The value indicating whether <paramref name="stream"/> should be disposed along with this instance,
    /// if <paramref name="mode"/> is <see cref="ZipperMode.Compress"/>.
    /// </param>
    /// <exception cref="ArgumentException"><paramref name="mode"/> is not <see cref="ZipperMode.Compress"/> nor <see cref="ZipperMode.Decompress"/>.</exception>
    /// <exception cref="IndexOutOfRangeException"><paramref name="blockSize"/> is out of range.</exception>
    public LZWStream(Stream stream, int blockSize, ZipperMode mode, bool leaveOpen = false)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(blockSize, MinBlockSize);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(blockSize, MaxBlockSize);

        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentException($"Value was neither {ZipperMode.Compress} nor {ZipperMode.Decompress}", nameof(mode));
        }

        if (mode == ZipperMode.Compress)
        {
            writer = new(stream, blockSize);
        }
        else
        {
            reader = new(stream);
        }

        this.stream = stream;
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
    /// Gets a value indicating whether the stream supports reading.
    /// </summary>
    /// <inheritdoc/>
    public override bool CanWrite => mode == ZipperMode.Compress && stream.CanWrite;

    /// <summary>
    /// Gets a value indicating whether the stream supports reading.
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
            Debug.Assert(writer != null, "Writer is null");
            writer.Flush();
        }
    }

    /// <summary>
    /// Reads data from underlying stream, decompresses it and writes to <paramref name="buffer"/>.
    /// </summary>
    /// <param name="buffer">Buffer to write decompressed data to.</param>
    /// <param name="offset">How many bytes to skip before reading from <paramref name="buffer"/>.</param>
    /// <param name="count">How many bytes to read from <paramref name="buffer"/>.</param>
    /// <exception cref="InvalidOperationException">Stream is set to <see cref="ZipperMode.Decompress"/> mode.</exception>
    /// <exception cref="ObjectDisposedException">Stream is disposed.</exception>
    public override void Write(byte[] buffer, int offset, int count)
        => Write(buffer.AsSpan(offset, count));

    /// <summary>
    /// Reads data from underlying stream, decompresses it and writes to <paramref name="buffer"/>.
    /// </summary>
    /// <param name="buffer">Buffer to write decompressed data to.</param>
    /// <param name="offset">How many bytes to skip before reading from <paramref name="buffer"/>.</param>
    /// <param name="count">How many bytes to read from <paramref name="buffer"/>.</param>
    /// <returns>Count of read bytes.</returns>
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

        Debug.Assert(writer != null, "Writer is null");

        writer.Write(buffer);
    }

    /// <inheritdoc cref="Read(byte[], int, int)"/>
    public override int Read(Span<byte> buffer)
    {
        EnsureNotClosed();
        EnsureMode(ZipperMode.Decompress);

        Debug.Assert(reader != null, "Reader is null");

        return reader.Read(buffer);
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
            if (mode == ZipperMode.Compress)
            {
                Debug.Assert(writer != null, "Writer is null");
                writer.Dispose();
            }
            else
            {
                Debug.Assert(reader != null, "Reader is null");
                reader.Dispose();
            }

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
