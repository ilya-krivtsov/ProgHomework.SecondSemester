namespace Zipper.Tests.Streams;

public interface IStreamProvider<TStream, TMode>
    where TStream : Stream
    where TMode : Enum
{
    public static abstract int MinBlockSize { get; }

    public static abstract int MaxBlockSize { get; }

    public static abstract TMode WritingMode { get; }

    public static abstract TMode ReadingMode { get; }

    public static abstract TMode UndefinedMode { get; }

    public static abstract TStream CreateStream(Stream stream, int blockSize, TMode mode, bool leaveOpen = false);

    public static abstract TStream CreateStream(Stream stream, TMode mode, bool leaveOpen = false);
}
