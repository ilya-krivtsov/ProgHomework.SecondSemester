namespace Zipper.Tests.LZW;

using System.Text;
using Zipper.LZW;

public class LZWStreamReadWriteTests
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
        var testData = LZWTestsSource.Data;

        using (var compressor = new LZWStream(stream, ZipperMode.Compress, true))
        {
            compressor.Write(testData);
        }

        DecompressData_And_AssertThat_ItIsCorrect(testData, readBufferSize);
    }

    [Test]
    public void Flush_ShouldNotAffect_DataToBeRead([ValueSource(nameof(BufferSizes))] int readWriteBufferSize)
    {
        var testData = LZWTestsSource.Data.AsSpan();

        using (var compressor = new LZWStream(stream, ZipperMode.Compress, true))
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

    private void DecompressData_And_AssertThat_ItIsCorrect(ReadOnlySpan<byte> testData, int readBufferSize)
    {
        stream.Seek(0, SeekOrigin.Begin);

        using var decompressor = new LZWStream(stream, ZipperMode.Decompress, true);
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
