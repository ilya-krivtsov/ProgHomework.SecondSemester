namespace Zipper;

/// <summary>
/// Reads unsigned integers of arbitrary width.
/// </summary>
internal class ArbitraryBitReader
{
    private readonly Stream stream;
    private readonly int width;
    private byte buffer;
    private int? bitsReadFromBuffer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArbitraryBitReader"/> class.
    /// </summary>
    /// <param name="stream">Stream to write to.</param>
    /// <param name="width">Width of integers between 4 and 32 bits.</param>
    public ArbitraryBitReader(Stream stream, int width)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(width, 4, nameof(width));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(width, 32, nameof(width));

        this.stream = stream;
        this.width = width;
        bitsReadFromBuffer = null;
    }

    /// <summary>
    /// Reads number from underlying stream and stores it in the <paramref name="number"/>.
    /// </summary>
    /// <param name="number">When this method returns, contains the value that was read, if read successfully, zero otherwise.</param>
    /// <returns><see langword="true"/> if <paramref name="number"/> was successfuly read, <see langword="true"/> otherwise.</returns>
    public bool ReadNext(out uint number)
    {
        number = 0;

        int remainingWidth = width;
        while (remainingWidth > 0)
        {
            if (bitsReadFromBuffer is null or >= 8)
            {
                int readByte = stream.ReadByte();
                if (readByte == -1)
                {
                    return false;
                }

                buffer = (byte)readByte;
                bitsReadFromBuffer = 0;
            }

            int remainingBitsToRead = 8 - bitsReadFromBuffer.Value;
            uint mask = 0xFFu >> bitsReadFromBuffer.Value;
            uint toWrite = (buffer & mask) >> Math.Max(0, remainingBitsToRead - remainingWidth);

            int previousRemainingWidth = remainingWidth;
            remainingWidth -= remainingBitsToRead;
            remainingWidth = Math.Max(0, remainingWidth);
            bitsReadFromBuffer += previousRemainingWidth - remainingWidth;
            number |= toWrite << remainingWidth;
        }

        return true;
    }
}
