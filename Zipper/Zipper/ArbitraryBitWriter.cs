namespace Zipper;

using System.Buffers;

/// <summary>
/// Writes unsigned integers of arbitrary width.
/// </summary>
internal class ArbitraryBitWriter : IDisposable
{
    /// <summary>
    /// Smallest allowed width of numbers.
    /// </summary>
    public const int MinWidth = 4;

    /// <summary>
    /// Largest allowed width of numbers.
    /// </summary>
    public const int MaxWidth = 32;

    private static readonly ArrayPool<byte> BufferPool = ArrayPool<byte>.Create();

    private readonly Stream stream;
    private readonly int width;
    private readonly byte[] buffer;
    private readonly bool leaveOpen;
    private int bitsWrittenInBuffer;
    private bool disposed = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArbitraryBitWriter"/> class.
    /// </summary>
    /// <param name="stream">Stream to write to.</param>
    /// <param name="width">Width of integers between <see cref="MinWidth"/> and <see cref="MinWidth"/> bits.</param>
    /// <param name="leaveOpen"><see langword="true"/> to leave the <paramref name="stream"/> open after disposing the <see cref="ArbitraryBitWriter"/> object, <see langword="false"/> otherwise.</param>
    public ArbitraryBitWriter(Stream stream, int width, bool leaveOpen = false)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(width, MinWidth, nameof(width));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(width, MaxWidth, nameof(width));

        if (!stream.CanWrite)
        {
            throw new ArgumentException("Stream does not support writing", nameof(stream));
        }

        this.stream = stream;
        this.width = width;
        this.leaveOpen = leaveOpen;
        buffer = BufferPool.Rent(width);
        bitsWrittenInBuffer = 0;
    }

    /// <summary>
    /// Writes <paramref name="number"/> to the underlying stream.
    /// </summary>
    /// <param name="number">Number to write.</param>
    public void Write(uint number)
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        number &= 0xFFFFFFFF >> (32 - width);

        int remainingWidth = width;
        while (remainingWidth > 0)
        {
            int bufferOffset = bitsWrittenInBuffer / 8;
            int bitsWrittenToCurrentByte = bitsWrittenInBuffer % 8;
            int bitsRemainingInCurrentByte = 8 - bitsWrittenToCurrentByte;

            uint toWrite = number >> Math.Max(0, remainingWidth - bitsRemainingInCurrentByte);
            int previousRemainingWidth = remainingWidth;
            remainingWidth -= bitsRemainingInCurrentByte;
            remainingWidth = Math.Max(0, remainingWidth);

            int bitsToBeWritten = previousRemainingWidth - remainingWidth;
            int bitsToBeLeftInCurrentByte = bitsRemainingInCurrentByte - bitsToBeWritten;
            buffer[bufferOffset] |= (byte)(toWrite << bitsToBeLeftInCurrentByte);
            bitsWrittenInBuffer += bitsToBeWritten;
        }

        if (bitsWrittenInBuffer >= width * 8)
        {
            Flush();
        }
    }

    /// <summary>
    /// Flushes the internal buffer.
    /// </summary>
    public void Flush()
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        if (bitsWrittenInBuffer == 0)
        {
            return;
        }

        int bytesWrittenInBuffer = (int)Math.Ceiling(bitsWrittenInBuffer / 8f);
        stream.Write(buffer, 0, bytesWrittenInBuffer);

        Array.Clear(buffer);
        bitsWrittenInBuffer = 0;
    }

    /// <summary>
    /// Releases all resources used by the current instance of the <see cref="ArbitraryBitWriter"/> class.
    /// </summary>
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        Flush();
        BufferPool.Return(buffer);

        if (!leaveOpen)
        {
            stream.Dispose();
        }

        disposed = true;
    }
}
