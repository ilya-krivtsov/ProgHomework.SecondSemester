namespace Zipper.LZW;

/// <summary>
/// Block type used to mark blocks in <see cref="LZWStream"/>.
/// </summary>
internal enum BlockType : byte
{
    /// <summary>
    /// Treat block as usual.
    /// </summary>
    Default = 0,

    /// <summary>
    /// All blocks after this one should not expand code table.
    /// </summary>
    FixCodeTableSize = 1,

    /// <summary>
    /// This block was written after <see cref="LZWWriter.Dispose()"/> and is the last one to be read.
    /// </summary>
    EndOfStream = 2,

    /// <summary>
    /// This block was written after <see cref="LZWWriter.Flush()"/>.
    /// </summary>
    Flush = 3,
}
