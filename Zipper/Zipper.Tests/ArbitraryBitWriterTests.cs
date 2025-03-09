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
        writer = new(memory, width, true);
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
    public void Writer_Flush_ShouldDoNothing_IfBufferIsEmpty()
    {
        // buffer is filled on every eight Write()
        for (int i = 0; i < 8; i++)
        {
            writer.Write(i * i);
        }

        var position = memory.Position;

        writer.Flush();
        Assert.That(memory.Position, Is.EqualTo(position));
    }

    [Test]
    public void Writer_Flush_ShouldFlushHalfFilledBuffer()
    {
        // buffer is filled on every eight Write(), so write only 4 numbers
        for (int i = 0; i < 4; i++)
        {
            writer.Write(i * i);
        }

        var position = memory.Position;

        writer.Flush();
        Assert.That(memory.Position, Is.Not.EqualTo(position));
    }

    [Test]
    public void Writer_Dispose_ShouldDisposeStream_IfLeaveOpenWasInitializedWith_False()
    {
        var closingWriter = new ArbitraryBitWriter(memory, width, false);

        closingWriter.Dispose();

        // getting any property should throw if stream was disposed
        Assert.Throws<ObjectDisposedException>(() => _ = memory.Position);
    }

    [Test]
    public void Writer_Dispose_ShouldNotDisposeStream_IfLeaveOpenWasInitializedWith_True()
    {
        writer.Dispose();

        // getting any property should not throw if stream was disposed
        Assert.DoesNotThrow(() => _ = memory.Position);
    }

    [Test]
    public void Writer_Write_ShouldThrow_IfDisposed()
    {
        writer.Write(123);

        writer.Dispose();

        Assert.Throws<ObjectDisposedException>(() => writer.Write(456));
    }

    [Test]
    public void Writer_Flush_ShouldThrow_IfDisposed()
    {
        writer.Write(123);

        writer.Dispose();

        Assert.Throws<ObjectDisposedException>(writer.Flush);
    }
}
