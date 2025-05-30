namespace Zipper.Tests.Streams;

internal class ZipperStreamTests
{
    public class ZipperStreamProviderWithoutBwt : IStreamProvider<ZipperStream, ZipperMode>
    {
        public static int MinBlockSize => ZipperStream.MinBlockSize;

        public static int MaxBlockSize => ZipperStream.MaxBlockSize;

        public static ZipperMode WritingMode => ZipperMode.Compress;

        public static ZipperMode ReadingMode => ZipperMode.Decompress;

        public static ZipperMode UndefinedMode => ZipperMode.Decompress + 100;

        public static ZipperStream CreateStream(Stream stream, int blockSize, ZipperMode mode, bool leaveOpen = false)
            => new(stream, blockSize, mode, leaveOpen, false);

        public static ZipperStream CreateStream(Stream stream, ZipperMode mode, bool leaveOpen = false)
            => new(stream, mode, leaveOpen, false);
    }

    public class ZipperStreamProviderWithBwt : IStreamProvider<ZipperStream, ZipperMode>
    {
        public static int MinBlockSize => ZipperStream.MinBlockSize;

        public static int MaxBlockSize => ZipperStream.MaxBlockSize;

        public static ZipperMode WritingMode => ZipperMode.Compress;

        public static ZipperMode ReadingMode => ZipperMode.Decompress;

        public static ZipperMode UndefinedMode => ZipperMode.Decompress + 100;

        public static ZipperStream CreateStream(Stream stream, int blockSize, ZipperMode mode, bool leaveOpen = false)
            => new(stream, blockSize, mode, leaveOpen, true);

        public static ZipperStream CreateStream(Stream stream, ZipperMode mode, bool leaveOpen = false)
            => new(stream, mode, leaveOpen, true);
    }

    public class ZipperStreamWithoutBwtExceptionsTests : StreamExceptionsTests<ZipperStream, ZipperMode, ZipperStreamProviderWithoutBwt>
    {
    }

    public class ZipperStreamWithoutBwtReadWriteTests : StreamReadWriteTests<ZipperStream, ZipperMode, ZipperStreamProviderWithoutBwt>
    {
    }

    public class ZipperStreamWithBwtExceptionsTests : StreamExceptionsTests<ZipperStream, ZipperMode, ZipperStreamProviderWithBwt>
    {
    }

    public class ZipperStreamWithBwtReadWriteTests : StreamReadWriteTests<ZipperStream, ZipperMode, ZipperStreamProviderWithBwt>
    {
    }
}
