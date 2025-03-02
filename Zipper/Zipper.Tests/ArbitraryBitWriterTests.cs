namespace Zipper.Tests;

public class ArbitraryBitWriterTests
{
    private readonly int width = 11;
    private MemoryStream memory;
    private ArbitraryBitWriter writer;

    [SetUp]
    public void Setup()
    {
        memory = new MemoryStream();
        writer = new(memory, width);
    }

    [TearDown]
    public void Teardown()
    {
        writer.Dispose();
        memory.Dispose();
    }

    [Test]
    public void Writer_Dispose_ShouldDoNothing_IfCalledTwice()
    {
        writer.Write(123);

        writer.Dispose();
        var position = memory.Position;

        writer.Dispose();
        Assert.That(memory.Position, Is.EqualTo(position));
    }

    [Test]
    public void Writer_Dispose_ShouldDoNothing_IfBufferIsEmpty()
    {
        // buffer is filled on every eight Write()
        for (int i = 0; i < 8; i++)
        {
            writer.Write(i * i);
        }

        var position = memory.Position;

        writer.Dispose();
        Assert.That(memory.Position, Is.EqualTo(position));
    }

    [Test]
    public void Writer_Dispose_ShouldDoNothing_IfCalledWithFalse()
    {
        var writer = new TestWriter(memory, width);

        for (int i = 0; i < 5; i++)
        {
            writer.Write(i * i);
        }

        var position = memory.Position;

        writer.Dispose();
        Assert.That(memory.Position, Is.EqualTo(position));
    }

    [Test]
    public void Writer_Write_ShouldThrow_IfDisposed()
    {
        writer.Write(123);

        writer.Dispose();

        Assert.Throws<ObjectDisposedException>(() => writer.Write(456));
    }

    private class TestWriter(Stream stream, int width) : ArbitraryBitWriter(stream, width)
    {
        protected override void Dispose(bool disposing)
        {
            base.Dispose(false);
        }
    }
}
