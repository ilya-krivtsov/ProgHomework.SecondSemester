namespace Zipper;

/// <summary>
/// Specifies whether to transform data to or reconstruct data from the underlying stream in <see cref="BWTStream"/>.
/// </summary>
public enum BWTMode
{
    /// <summary>
    /// Transform data and write it to the underlying stream.
    /// </summary>
    Transform,

    /// <summary>
    /// Read data from the underlying stream and reconstruct it.
    /// </summary>
    Reconstruct,
}
