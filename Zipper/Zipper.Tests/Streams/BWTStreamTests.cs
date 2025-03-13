namespace Zipper.Tests.Streams;

public class BWTStreamTests
{
    public class BWTStreamProvider : IStreamProvider<BWTStream, BWTMode>
    {
        public static int MinBlockSize => BWTStream.MinBlockSize;

        public static int MaxBlockSize => BWTStream.MaxBlockSize;

        public static BWTMode WritingMode => BWTMode.Transform;

        public static BWTMode ReadingMode => BWTMode.Reconstruct;

        public static BWTMode UndefinedMode => BWTMode.Reconstruct + 100;

        public static BWTStream CreateStream(Stream stream, int blockSize, BWTMode mode, bool leaveOpen = false)
            => new(stream, blockSize, mode, leaveOpen);

        public static BWTStream CreateStream(Stream stream, BWTMode mode, bool leaveOpen = false)
            => new(stream, mode, leaveOpen);
    }

    public class BWTStreamExceptionsTests : StreamExceptionsTests<BWTStream, BWTMode, BWTStreamProvider>
    {
    }

    public class BWTStreamReadWriteTests : StreamReadWriteTests<BWTStream, BWTMode, BWTStreamProvider>
    {
    }
}
