namespace BurrowsWheelerTransform.Tests;

public class TransformResultTests
{
    [Test]
    public void TransformResultConstructor_ShouldNotThrow()
    {
        Assert.DoesNotThrow(() => new TransformResult(string.Empty, -1));
        Assert.DoesNotThrow(() => new TransformResult("A", 0));
        Assert.DoesNotThrow(() => new TransformResult("ABCDEF", 3));
        Assert.DoesNotThrow(() => new TransformResult("QWERTY", 5));
    }

    [Test]
    public void TransformResultConstructor_ShouldThrow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TransformResult(string.Empty, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new TransformResult("ABCD", -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new TransformResult("ABCD", 4));
    }
}
