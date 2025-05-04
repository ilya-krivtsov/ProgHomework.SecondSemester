namespace Zipper;

/// <summary>
/// Specifies whether to compress data to or decompress data from the underlying stream.
/// </summary>
public enum ZipperMode
{
    /// <summary>
    /// Compress data to the underlying stream.
    /// </summary>
    Compress,

    /// <summary>
    /// Decompress data from the underlying stream.
    /// </summary>
    Decompress,
}
