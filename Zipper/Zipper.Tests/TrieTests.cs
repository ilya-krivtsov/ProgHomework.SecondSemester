namespace Zipper.Tests;

public class TrieTests
{
    private static readonly byte[][] TestStrings =
    [
        .. new string[]
        {
            string.Empty,
            "A",
            "AB",
            "ABC",
            "ABCD",
        }
        .Select(System.Text.Encoding.UTF8.GetBytes)
    ];

    private static readonly byte[][][] TestStringsSet =
    [
        TestStrings,
        [.. TestUtil.GetRandomStrings()]
    ];

    private Trie trie;

    [SetUp]
    public void Setup()
    {
        trie = new();
    }
}
