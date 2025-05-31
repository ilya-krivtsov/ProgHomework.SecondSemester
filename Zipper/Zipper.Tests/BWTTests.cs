namespace Zipper.Tests;

public class BWTTests
{
    private static readonly string[] StringTestData =
    [
        string.Empty,

        "A",
        "BB",
        "CCCCCC",
        "ABACABA",
        "ABABABABAB",
    ];

    private static readonly byte[][] TestData =
    [
        .. StringTestData.Select(System.Text.Encoding.UTF8.GetBytes),
        .. TestUtil.GetRandomStrings()
    ];

    [Test]
    public void InverseTransform_ShouldBe_SameAs_Input([ValueSource(nameof(TestData))] byte[] input)
    {
        Span<byte> transformed = stackalloc byte[input.Length];
        var index = BWT.ForwardTransform(input, transformed);

        Span<byte> reconstructed = stackalloc byte[input.Length + 16];
        BWT.InverseTransform(transformed, index, reconstructed);
        Assert.That(reconstructed[..input.Length].SequenceEqual(input), Is.True);
    }

    [Test]
    public void Transform_ShouldThrowIf_InputIsLargerThanOutput()
    {
        int inputLength = 16;
        int outputLength = inputLength - 1;
        Assert.Throws<ArgumentOutOfRangeException>(() => BWT.ForwardTransform(stackalloc byte[inputLength], stackalloc byte[outputLength]));
    }

    [Test]
    public void InverseTransform_ShouldThrowIf_InputIsLargerThanOutput()
    {
        int inputLength = 16;
        int outputLength = inputLength - 1;
        Assert.Throws<ArgumentOutOfRangeException>(() => BWT.InverseTransform(stackalloc byte[inputLength], 0, stackalloc byte[outputLength]));
    }

    [Test]
    public void InverseTransform_ShouldThrowIf_IdentityIndexIsNegative()
    {
        int length = 16;
        Assert.Throws<ArgumentOutOfRangeException>(() => BWT.InverseTransform(stackalloc byte[length], -1, stackalloc byte[length]));
    }

    [Test]
    public void InverseTransform_ShouldThrowIf_IdentityIndexIs_GreaterThanOrEqualTo_InputLength()
    {
        int length = 16;
        Assert.Throws<ArgumentOutOfRangeException>(() => BWT.InverseTransform(stackalloc byte[length], length + 1, stackalloc byte[length]));
    }
}
