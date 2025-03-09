namespace Zipper.LZW;

using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;

/// <summary>
/// Internal class used to write compressed data to stream.
/// </summary>
internal class LZWWriter : IDisposable
{
    private const int DataOffset = 4;
    private const int MaxCodesCount = (320 * 1024) - 1;

    private static readonly ArrayPool<byte> BlockPool = ArrayPool<byte>.Create();

    private readonly Stream stream;
    private readonly int blockSize;
    private readonly byte[] block;
    private readonly MemoryStream memory;
    private readonly Trie<int> trie;

    private ArbitraryBitWriter writer;
    private int bitsWrittenInBlock;
    private bool disableCodeTableExpansion;

    private int blockIndex = 0;
    private int bytesRead = 0;

    private int codeWidth;
    private int codesCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="LZWWriter"/> class.
    /// </summary>
    /// <param name="stream">Stream to write to.</param>
    /// <param name="blockSize">The internal block size to use.</param>
    public LZWWriter(Stream stream, int blockSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(blockSize, LZWStream.MinBlockSize);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(blockSize, LZWStream.MaxBlockSize);

        this.stream = stream;
        this.blockSize = blockSize;

        block = BlockPool.Rent(blockSize);
        memory = new(block);
        memory.Seek(DataOffset, SeekOrigin.Begin);
        bitsWrittenInBlock = DataOffset * 8;

        codeWidth = 8;
        codesCount = 1 << codeWidth;

        trie = new();
        for (int i = 0; i < codesCount; i++)
        {
            trie.AddChild((byte)i, i);
        }

        writer = new(memory, codeWidth, true);
    }

    /// <summary>
    /// Compresses data in <paramref name="buffer"/> and writes it to underlying stream.
    /// </summary>
    /// <param name="buffer">Buffer to read data from.</param>
    public void Write(ReadOnlySpan<byte> buffer)
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            bytesRead++;
            byte value = buffer[i];
            if (trie.AtRoot)
            {
                trie.MoveForward(value);
                continue;
            }

            if (!trie.HasChild(value))
            {
                if (!disableCodeTableExpansion)
                {
                    trie.AddChild(value, codesCount);
                }

                bool bufferOverflow = bitsWrittenInBlock + codeWidth > blockSize * 8;

                bool shouldIncrementCodeWidth = false;
                bool shouldDisableCodeTableExpansion = false;
                if (!disableCodeTableExpansion)
                {
                    codesCount++;
                    shouldDisableCodeTableExpansion = codesCount >= MaxCodesCount;
                    shouldIncrementCodeWidth = codesCount >= 1 << codeWidth;
                }

                if (shouldDisableCodeTableExpansion)
                {
                    disableCodeTableExpansion = true;
                    FlushInternal(BlockType.Default);
                    FlushInternal(BlockType.FixCodeTableSize);
                }
                else if (bufferOverflow || shouldIncrementCodeWidth)
                {
                    FlushInternal(BlockType.Default);
                }

                if (shouldIncrementCodeWidth)
                {
                    codeWidth++;

                    writer.Dispose();
                    writer = new(memory, codeWidth, true);
                }

                writer.Write(trie.CurrentValue);
                bitsWrittenInBlock += codeWidth;

                trie.Reset();
            }

            trie.MoveForward(value);
        }
    }

    /// <summary>
    /// Writes all pending data to the underlying stream.
    /// </summary>
    public void Flush()
    {
        FlushInternal(BlockType.Default);

        writer.Write(trie.CurrentValue);
        bitsWrittenInBlock += codeWidth;

        trie.Reset();

        FlushInternal(BlockType.Default);
    }

    /// <summary>
    /// Writes all pending data to the underlying stream and disposes internal buffers.
    /// </summary>
    public void Dispose()
    {
        Flush();
        FlushInternal(BlockType.EndOfStream);

        memory.Dispose();
        BlockPool.Return(block);
    }

    private void FlushInternal(BlockType type)
    {
        Debug.Assert(codeWidth <= 32, "Code width too large");
        Debug.Assert(Enum.IsDefined(type), $"Unknown {nameof(BlockType)} parameter");

        if (type == BlockType.FixCodeTableSize)
        {
            var binWriter = new BinaryWriter(memory);
            binWriter.Write(MaxCodesCount);
            binWriter.Flush();

            Console.WriteLine($"disable code table expansion @ block #{blockIndex}");
        }

        writer.Flush();
        int length = (int)memory.Position;
        ushort dataLength = (ushort)(length - DataOffset);

        if (!(dataLength == 0 && type == BlockType.Default))
        {
            block[0] = (byte)type;
            block[1] = (byte)codeWidth;
            BinaryPrimitives.WriteUInt16LittleEndian(block.AsSpan()[2..4], dataLength);

            stream.Write(block, 0, length);
            stream.Flush();

            if (blockIndex < 20 || (blockIndex % 1024) == 0 || type == BlockType.FixCodeTableSize)
            {
                Console.WriteLine($"wrote block #{blockIndex}, read: {bytesRead / 1024.0:0.00} K, block size: {dataLength}, codeWidth: {codeWidth}, used codes: {codesCount}");
            }

            blockIndex++;
        }

        Array.Clear(block);

        memory.Seek(DataOffset, SeekOrigin.Begin);
        bitsWrittenInBlock = DataOffset * 8;
    }
}
