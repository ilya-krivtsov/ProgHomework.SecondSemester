namespace Zipper;

/// <summary>
/// Writes unsigned integers of arbitrary width.
/// </summary>
internal class ArbitraryBitWriter : IDisposable
{
    private readonly Stream stream;
    private readonly int width;
    private readonly byte[] buffer;
    private int bitsWrittenInBuffer;
    private bool disposed = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArbitraryBitWriter"/> class.
    /// </summary>
    /// <param name="stream">Stream to write to.</param>
    /// <param name="width">Width of integers between 4 and 32 bits.</param>
    public ArbitraryBitWriter(Stream stream, int width)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(width, 4, nameof(width));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(width, 32, nameof(width));

        this.stream = stream;
        this.width = width;
        buffer = new byte[width];
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

        if (bitsWrittenInBuffer >= buffer.Length * 8)
        {
            stream.Write(buffer);
            Array.Clear(buffer);
            bitsWrittenInBuffer = 0;
        }
    }

    /// <summary>
    /// Releases all resources used by the current instance of the <see cref="ArbitraryBitWriter"/> class.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="ArbitraryBitWriter"/> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        disposed = true;

        if (!disposing)
        {
            return;
        }

        if (bitsWrittenInBuffer == 0)
        {
            return;
        }

        int bytesWrittenInBuffer = (int)Math.Ceiling(bitsWrittenInBuffer / 8f);
        stream.Write(buffer.AsSpan()[..bytesWrittenInBuffer]);
    }
}
