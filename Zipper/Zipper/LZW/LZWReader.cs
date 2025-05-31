namespace Zipper.LZW;

using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;

/// <summary>
/// Internal class used to read compressed data from stream.
/// </summary>
internal class LZWReader : IDisposable
{
    private static readonly ArrayPool<byte> BlockPool = ArrayPool<byte>.Create();

    private readonly Stream stream;
    private readonly Dictionary<int, byte[]> storedCodes;
    private MemoryStream? memory;
    private byte[]? block;
    private int blockSize;
    private bool endOfStreamReached;
    private bool flushed;

    private byte[]? word;
    private int wordPosition;

    private ArbitraryBitReader? reader;
    private int lastWordCode;
    private int maxCodesCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="LZWReader"/> class.
    /// </summary>
    /// <param name="stream">Stream to read from.</param>
    public LZWReader(Stream stream)
    {
        if (!stream.CanRead)
        {
            throw new ArgumentException("Stream does not support reading", nameof(stream));
        }

        this.stream = stream;

        storedCodes = [];
        for (int i = 0; i < 256; i++)
        {
            storedCodes[i] = [(byte)i];
        }

        lastWordCode = 256;
        maxCodesCount = int.MaxValue;
    }

    /// <summary>
    /// Reads data from underlying stream, decompresses it and writes to <paramref name="buffer"/>.
    /// </summary>
    /// <param name="buffer">Buffer to write decompressed data to.</param>
    /// <returns>Count of read bytes.</returns>
    /// <exception cref="EndOfStreamException">Unexpected end of stream.</exception>
    /// <exception cref="InvalidDataException">Invalid data stream.</exception>
    public int Read(Span<byte> buffer)
    {
        int bufferPosition = 0;

        while (bufferPosition < buffer.Length)
        {
            // write leftover word from previous (iteration) or (Read() call)
            if (word != null)
            {
                int wordLength = Math.Min(buffer.Length - bufferPosition, word.Length - wordPosition);
                word.AsSpan().Slice(wordPosition, wordLength).CopyTo(buffer[bufferPosition..]);

                bufferPosition += wordLength;
                wordPosition += wordLength;
                if (bufferPosition >= buffer.Length)
                {
                    break;
                }

                word = null;
            }

            if (block == null && !TryReadBuffer())
            {
                return 0;
            }

            Debug.Assert(block != null, "Block is null");
            Debug.Assert(memory != null, "Memory is null");
            Debug.Assert(reader != null, "Reader is null");

            if (!reader.ReadNext(out int code))
            {
                if (endOfStreamReached)
                {
                    break;
                }

                if (blockSize == 0)
                {
                    block = null;
                    continue;
                }

                throw new EndOfStreamException();
            }

            if (!storedCodes.TryGetValue(code, out var readWord))
            {
                throw new InvalidDataException();
            }

            word = readWord;
            wordPosition = 0;

            if (lastWordCode <= maxCodesCount)
            {
                if (storedCodes.TryGetValue(lastWordCode, out var incompleteWord))
                {
                    incompleteWord[^1] = word[0];
                    lastWordCode++;
                }

                if (!flushed)
                {
                    var newWord = new byte[word.Length + 1];
                    word.CopyTo(newWord, 0);
                    storedCodes[lastWordCode] = newWord;
                }
            }

            if (memory.Position >= blockSize)
            {
                BlockPool.Return(block);
                block = null;
            }
        }

        return bufferPosition;
    }

    /// <summary>
    /// Disposes internal buffers.
    /// </summary>
    public void Dispose()
    {
        if (block != null)
        {
            BlockPool.Return(block);
        }
    }

    private bool TryReadBuffer()
    {
        int headerSize = 6;
        Span<byte> header = stackalloc byte[headerSize];
        if (stream.Read(header) != headerSize)
        {
            return false;
        }

        var blockType = (BlockType)header[0];
        var codeWidth = header[1];

        blockSize = BinaryPrimitives.ReadInt32LittleEndian(header[2..6]);
        block = BlockPool.Rent(blockSize);

        if (stream.Read(block, 0, blockSize) != blockSize)
        {
            throw new EndOfStreamException();
        }

        switch (blockType)
        {
            case BlockType.Default:
                flushed = false;
                break;

            case BlockType.FixCodeTableSize:
                maxCodesCount = BinaryPrimitives.ReadInt32LittleEndian(block);
                return TryReadBuffer();

            case BlockType.EndOfStream:
                endOfStreamReached = true;
                break;

            case BlockType.Flush:
                flushed = true;
                break;

            default:
                throw new InvalidDataException();
        }

        memory = new(block);
        reader = new(memory, codeWidth);

        return true;
    }
}
