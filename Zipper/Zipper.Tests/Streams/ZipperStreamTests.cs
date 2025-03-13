namespace Zipper.Tests.Streams;

internal class ZipperStreamTests
{
    public class ZipperStreamProvider : IStreamProvider<ZipperStream, ZipperMode>
    {
        public static int MinBlockSize => ZipperStream.MinBlockSize;

        public static int MaxBlockSize => ZipperStream.MaxBlockSize;

        public static ZipperMode WritingMode => ZipperMode.Compress;

        public static ZipperMode ReadingMode => ZipperMode.Decompress;

        public static ZipperMode UndefinedMode => ZipperMode.Decompress + 100;

        public static ZipperStream CreateStream(Stream stream, int blockSize, ZipperMode mode, bool leaveOpen = false)
            => new(stream, blockSize, mode, leaveOpen);

        public static ZipperStream CreateStream(Stream stream, ZipperMode mode, bool leaveOpen = false)
            => new(stream, mode, leaveOpen);
    }

    public class ZipperStreamExceptionsTests : StreamExceptionsTests<ZipperStream, ZipperMode, ZipperStreamProvider>
    {
    }

    public class ZipperStreamReadWriteTests : StreamReadWriteTests<ZipperStream, ZipperMode, ZipperStreamProvider>
    {
    }
}
