namespace Zipper.Tests;

public class ArbitraryBitReaderWriterTests
{
    private static readonly TestData[] TestDataSource = GenerateData();

    [Test]
    public void ReaderAndWriter_ShouldThrowIf_InitializedWith_Width_InDisallowedRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ArbitraryBitReader(Stream.Null, ArbitraryBitReader.MinWidth - 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ArbitraryBitReader(Stream.Null, ArbitraryBitReader.MaxWidth + 1));

        Assert.Throws<ArgumentOutOfRangeException>(() => new ArbitraryBitWriter(Stream.Null, ArbitraryBitWriter.MinWidth - 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ArbitraryBitWriter(Stream.Null, ArbitraryBitWriter.MaxWidth + 1));
    }

    [Test]
    public void Reader_ShouldThrowIf_StreamCanNotRead()
        => Assert.Throws<ArgumentException>(() => new ArbitraryBitReader(new TestStream(), ArbitraryBitReader.MinWidth));

    [Test]
    public void Writer_ShouldThrowIf_StreamCanNotWrite()
        => Assert.Throws<ArgumentException>(() => new ArbitraryBitWriter(new TestStream(), ArbitraryBitWriter.MinWidth));

    [Test]
    public void Reader_ShouldReadValues_WrittenBy_Writer_Correctly([ValueSource(nameof(TestDataSource))] TestData data)
    {
        int memorySize = (int)Math.Ceiling(data.Width * data.Numbers.Length / 8f);
        var backingMemory = new byte[memorySize];

        using (var memory = new MemoryStream(backingMemory))
        {
            using var writer = new ArbitraryBitWriter(memory, data.Width);
            for (int i = 0; i < data.Numbers.Length; i++)
            {
                writer.Write(data.Numbers[i]);
            }
        }

        using (var memory = new MemoryStream(backingMemory))
        {
            var reader = new ArbitraryBitReader(memory, data.Width);
            for (int i = 0; i < data.Numbers.Length; i++)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(reader.ReadNext(out int number), Is.True);
                    Assert.That(number, Is.EqualTo(data.Numbers[i]));
                });
            }
        }
    }

    [Test]
    public void Reader_ReadNext_ShouldReturnFalse_WhenNoDataAvailable()
    {
        int width = 7;
        int numbersCount = 5;
        int memorySize = (int)Math.Ceiling(width * numbersCount / 8f);
        var backingMemory = new byte[memorySize];

        using (var memory = new MemoryStream(backingMemory))
        {
            using var writer = new ArbitraryBitWriter(memory, width);
            for (int i = 0; i < numbersCount; i++)
            {
                writer.Write(i * i);
            }
        }

        using (var memory = new MemoryStream(backingMemory))
        {
            var reader = new ArbitraryBitReader(memory, width);
            for (int i = 0; i < numbersCount; i++)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(reader.ReadNext(out int number), Is.True);
                    Assert.That(number, Is.EqualTo(i * i));
                });
            }

            Assert.That(reader.ReadNext(out _), Is.False);
        }
    }

    private static TestData[] GenerateData()
    {
        var random = new Random(872375823);

        int minWidth = 4;
        int maxWidth = 32;
        int numbersLength = 21;

        var result = new TestData[maxWidth - minWidth + 1];
        for (int i = 0; i < result.Length; i++)
        {
            int width = i + minWidth;
            long upperBound = 1L << width;

            var numbers = new int[numbersLength];
            for (int j = 0; j < numbersLength; j++)
            {
                numbers[j] = (int)random.NextInt64(upperBound);
            }

            result[i] = new(width, numbers);
        }

        return result;
    }

    public readonly record struct TestData(int Width, int[] Numbers);

    // use MemoryStream, because implementing all Stream's abstract membrers leads to bad code coverage
    private class TestStream : MemoryStream
    {
        public override bool CanRead => false;

        public override bool CanWrite => false;
    }
}
