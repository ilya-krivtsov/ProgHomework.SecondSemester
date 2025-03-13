namespace Zipper.Tests.Streams;

public abstract class StreamExceptionsTests<TStream, TMode, TProvider>
    where TStream : Stream
    where TMode : Enum
    where TProvider : IStreamProvider<TStream, TMode>
{
    private TStream compressor;
    private TStream decompressor;

    [SetUp]
    public void Setup()
    {
        compressor = TProvider.CreateStream(Stream.Null, TProvider.MinBlockSize, TProvider.WritingMode);
        decompressor = TProvider.CreateStream(Stream.Null, TProvider.MinBlockSize, TProvider.ReadingMode);
    }

    [TearDown]
    public void Teardown()
    {
        compressor.Dispose();
        decompressor.Dispose();
    }

    [Test]
    public void Constructor_ShouldThrowIf_BlockSize_IsIncorrect()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TProvider.CreateStream(Stream.Null, TProvider.MinBlockSize - 1, TProvider.WritingMode));
        Assert.Throws<ArgumentOutOfRangeException>(() => TProvider.CreateStream(Stream.Null, TProvider.MaxBlockSize + 1, TProvider.WritingMode));
    }

    [Test]
    public void Constructor_ShouldThrowIf_Mode_IsNotDefined()
    {
        Assert.Throws<ArgumentException>(() => TProvider.CreateStream(Stream.Null, TProvider.UndefinedMode));
    }

    [Test]
    public void Constructor_ShouldThrowIf_Mode_IsCompress_And_Stream_CanNotWrite()
    {
        Assert.Throws<ArgumentException>(() => TProvider.CreateStream(new UnwriteableStream(), TProvider.WritingMode));
    }

    [Test]
    public void Constructor_ShouldThrowIf_Mode_IsDecompress_And_Stream_CanNotRead()
    {
        Assert.Throws<ArgumentException>(() => TProvider.CreateStream(new UnreadableStream(), TProvider.ReadingMode));
    }

    [Test]
    public void UnsupportedPropertiesAndMethods_ShouldThrow()
    {
        Assert.Throws<NotSupportedException>(() => _ = compressor.Length);

        Assert.Throws<NotSupportedException>(() => _ = compressor.Position);
        Assert.Throws<NotSupportedException>(() => compressor.Position = 0);

        Assert.Throws<NotSupportedException>(() => compressor.Seek(0, SeekOrigin.Begin));
        Assert.Throws<NotSupportedException>(() => compressor.SetLength(0));
    }

    [Test]
    public void CanRead_ShouldReturnFalse_AndCanWrite_ShouldReturnTrue_IfModeIs_Compress()
    {
        Assert.Multiple(() =>
        {
            Assert.That(compressor.CanRead, Is.False);
            Assert.That(compressor.CanWrite, Is.True);
        });
    }

    [Test]
    public void CanRead_ShouldReturnTrue_And_CanWrite_ShouldReturnFalse_IfModeIs_Decompress()
    {
        Assert.Multiple(() =>
        {
            Assert.That(decompressor.CanRead, Is.True);
            Assert.That(decompressor.CanWrite, Is.False);
        });
    }

    [Test]
    public void CanSeek_ShouldReturnFalse()
    {
        Assert.Multiple(() =>
        {
            Assert.That(compressor.CanSeek, Is.False);
            Assert.That(decompressor.CanSeek, Is.False);
        });
    }

    [Test]
    public void Write_ShouldThrow_IfModeIs_Decompress()
    {
        Assert.Throws<InvalidOperationException>(() => decompressor.Write([]));
        Assert.Throws<InvalidOperationException>(() => decompressor.Write([], 0, 0));
    }

    [Test]
    public void Read_ShouldThrow_IfModeIs_Compress()
    {
        Assert.Throws<InvalidOperationException>(() => _ = compressor.Read([]));
        Assert.Throws<InvalidOperationException>(() => _ = compressor.Read([], 0, 0));
    }

    [Test]
    public void AllMethodsShouldThrow_IfDisposed()
    {
        compressor.Dispose();
        decompressor.Dispose();

        Assert.Throws<ObjectDisposedException>(() => compressor.Write([]));
        Assert.Throws<ObjectDisposedException>(() => compressor.Write([], 0, 0));

        Assert.Throws<ObjectDisposedException>(() => _ = compressor.Read([]));
        Assert.Throws<ObjectDisposedException>(() => _ = compressor.Read([], 0, 0));

        Assert.Throws<ObjectDisposedException>(compressor.Flush);
    }

    [Test]
    public void ReadAndWrite_ShouldThrow_IfArgumentsAreIncorrect()
    {
        int length = 8;
        var buffer = new byte[length];

        Test(buffer, -1, length);
        Test(buffer, length, length);

        Test(buffer, 0, -1);
        Test(buffer, 0, length + 1);

        Test(buffer, length / 2, length);
        Test(buffer, 0, -1);

        void Test(byte[] buffer, int offset, int count)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => compressor.Write(buffer, offset, count));
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = decompressor.Read(buffer, offset, count));
        }
    }

    private class UnwriteableStream : MemoryStream
    {
        public override bool CanWrite => false;
    }

    private class UnreadableStream : MemoryStream
    {
        public override bool CanRead => false;
    }
}
