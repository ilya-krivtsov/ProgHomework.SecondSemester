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

    private Trie<int> trie;

    [SetUp]
    public void Setup()
    {
        trie = new();
    }

    [Test]
    public void TrieAdd_And_TrieTryGetValue_ReturnsCorrectly_SingleValue([ValueSource(nameof(TestStrings))] byte[] bytes)
    {
        TestAddValue(trie, bytes);
    }

    [Test]
    public void TrieAdd_And_TrieTryGetValue_ReturnsCorrectly_MultipleValues([ValueSource(nameof(TestStringsSet))] byte[][] strings)
    {
        foreach (var bytes in strings)
        {
            TestAddValue(trie, bytes);
        }
    }

    private static void TestAddValue(Trie<int> trie, byte[] bytes)
    {
        Assert.Multiple(() =>
        {
            Assert.That(trie.TryGetValue(bytes, out _), Is.False);

            Assert.That(Add(trie, bytes), Is.True);
            Assert.That(Add(trie, bytes), Is.False);

            Assert.That(trie.TryGetValue(bytes, out int value), Is.True);
            Assert.That(value, Is.EqualTo(bytes.Length));
        });
    }

    private static bool Add(Trie<int> trie, byte[] bytes)
        => trie.Add(bytes, bytes.Length);
}
