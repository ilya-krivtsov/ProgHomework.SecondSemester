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

        Span<byte> reconstructed = stackalloc byte[input.Length];
        BWT.InverseTransform(transformed, index, reconstructed);
        Assert.That(reconstructed.SequenceEqual(input), Is.True);
    }
}
