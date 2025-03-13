namespace Zipper.Tests.Streams;

public abstract class StreamReadWriteTests<TStream, TMode, TProvider>
    where TStream : Stream
    where TMode : Enum
    where TProvider : IStreamProvider<TStream, TMode>
{
    private static readonly int[] BufferSizes = [1, 2, 3, 7, 14, 19, 31, 63, 127, 255, 1023];

    private MemoryStream stream;

    [SetUp]
    public void Setup()
    {
        stream = new();
    }

    [Test]
    public void Read_ShouldReadData_WrittenBy_Write_Correctly([ValueSource(nameof(BufferSizes))] int readBufferSize)
    {
        var testData = GetData(StreamTestsSource.ImageData, readBufferSize);

        using (var compressor = TProvider.CreateStream(stream, TProvider.WritingMode, true))
        {
            compressor.Write(testData);
        }

        DecompressData_And_AssertThat_ItIsCorrect(testData, readBufferSize);
    }

    [Test]
    public void Flush_ShouldNotAffect_DataToBeRead([ValueSource(nameof(BufferSizes))] int readWriteBufferSize)
    {
        var testData = GetData(StreamTestsSource.TextData, readWriteBufferSize);

        using (var compressor = TProvider.CreateStream(stream, TProvider.WritingMode, true))
        {
            for (int offset = 0; offset < testData.Length; offset += readWriteBufferSize)
            {
                var dataSlice = testData.Slice(offset, Math.Min(readWriteBufferSize, testData.Length - offset));
                compressor.Write(dataSlice);
                compressor.Flush();
            }
        }

        DecompressData_And_AssertThat_ItIsCorrect(testData, readWriteBufferSize);
    }

    private static ReadOnlySpan<byte> GetData(ReadOnlySpan<byte> data, int bufferSize)
    {
        var relativeLength = Math.Clamp(bufferSize / (float)BufferSizes[^1], 0, 1);

        return data[..(int)Math.Ceiling(data.Length * relativeLength)];
    }

    private void DecompressData_And_AssertThat_ItIsCorrect(ReadOnlySpan<byte> testData, int readBufferSize)
    {
        stream.Seek(0, SeekOrigin.Begin);

        using var decompressor = TProvider.CreateStream(stream, TProvider.ReadingMode, true);
        int offset = 0;
        Span<byte> buffer = stackalloc byte[readBufferSize];

        while (true)
        {
            int bytesRead = decompressor.Read(buffer);

            Assert.That(bytesRead, Is.LessThanOrEqualTo(readBufferSize));

            if (bytesRead == 0)
            {
                break;
            }

            Assert.That(offset + bytesRead, Is.LessThanOrEqualTo(testData.Length));

            var slicedData = testData.Slice(offset, bytesRead);
            var slicedBuffer = buffer[..bytesRead];

            Assert.That(slicedBuffer.SequenceEqual(slicedData), Is.True);

            offset += bytesRead;
        }
    }
}
