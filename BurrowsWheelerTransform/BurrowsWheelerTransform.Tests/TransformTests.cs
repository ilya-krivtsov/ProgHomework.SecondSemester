namespace BurrowsWheelerTransform.Tests;

public class TransformTests
{
    private static readonly List<string> TestData =
    [
        string.Empty,

        "A",
        "BB",
        "CCCCCC",
        "ABACABA",
        "ABABABABAB",

        ..GetRandomStrings()
    ];

    [Test]
    public void InverseTransform_ShouldBe_SameAs_Input([ValueSource(nameof(TestData))] string input)
    {
        var result = Transform.ForwardTransform(input);
        var reconstructed = Transform.InverseTransform(result);
        Assert.That(input.SequenceEqual(reconstructed), Is.True);
    }

    private static IEnumerable<string> GetRandomStrings()
    {
        int seed = 1743658243;
        var random = new Random(seed);

        int steps = 16;
        int length = 256;
        char[] buffer = new char[length];

        for (int i = 0; i < steps; i++)
        {
            for (int j = 0; j < length; j++)
            {
                buffer[j] = (char)random.Next(' ', '~' + 1);
            }

            yield return new(buffer);
        }
    }
}
