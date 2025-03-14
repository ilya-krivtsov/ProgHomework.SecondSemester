namespace Zipper.Tests.Streams;

using Zipper.LZW;

public class LZWStreamTests
{
    public class LZWStreamProvider : IStreamProvider<LZWStream, ZipperMode>
    {
        public static int MinBlockSize => LZWStream.MinBlockSize;

        public static int MaxBlockSize => LZWStream.MaxBlockSize;

        public static ZipperMode WritingMode => ZipperMode.Compress;

        public static ZipperMode ReadingMode => ZipperMode.Decompress;

        public static ZipperMode UndefinedMode => ZipperMode.Decompress + 100;

        public static LZWStream CreateStream(Stream stream, int blockSize, ZipperMode mode, bool leaveOpen = false)
            => new(stream, blockSize, mode, leaveOpen);

        public static LZWStream CreateStream(Stream stream, ZipperMode mode, bool leaveOpen = false)
            => new(stream, mode, leaveOpen);
    }

    public class LZWStreamExceptionsTests : StreamExceptionsTests<LZWStream, ZipperMode, LZWStreamProvider>
    {
    }

    public class LZWStreamReadWriteTests : StreamReadWriteTests<LZWStream, ZipperMode, LZWStreamProvider>
    {
    }
}
