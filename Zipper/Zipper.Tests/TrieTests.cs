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

    [Test]
    public void TrieAdd_And_TrieRemove_ReturnsCorrectly_SingleValue([ValueSource(nameof(TestStrings))] byte[] item)
    {
        Assert.That(() => trie.Add(item), Is.True);
        Assert.That(() => trie.Add(item), Is.False);
        Assert.That(trie.Size, Is.EqualTo(1));

        Assert.That(() => trie.Remove(item), Is.True);
        Assert.That(() => trie.Remove(item), Is.False);
        Assert.That(trie.Size, Is.EqualTo(0));
    }

    [Test]
    public void TrieAdd_And_TrieRemove_ReturnsCorrectly_MultipleValues([ValueSource(nameof(TestStringsSet))] byte[][] strings)
    {
        for (int i = 0; i < strings.Length; i++)
        {
            var item = strings[i];
            Assert.That(() => trie.Add(item), Is.True);
            Assert.That(() => trie.Add(item), Is.False);
            Assert.That(trie.Size, Is.EqualTo(i + 1));
        }

        for (int i = strings.Length - 1; i >= 0; i--)
        {
            var item = strings[i];
            Assert.That(() => trie.Remove(item), Is.True);
            Assert.That(() => trie.Remove(item), Is.False);
            Assert.That(trie.Size, Is.EqualTo(i));
        }
    }

    [Test]
    public void TrieContains_IsCorrect([ValueSource(nameof(TestStrings))] byte[] item)
    {
        Assert.That(() => trie.Contains(item), Is.False);
        trie.Add(item);
        Assert.That(() => trie.Contains(item), Is.True);
        trie.Remove(item);
        Assert.That(() => trie.Contains(item), Is.False);
    }

    [Test]
    public void TrieHowManyStartsWithPrefix_IsCorrect()
    {
        int length = TestStrings.Length;
        for (int i = 0; i < length; i++)
        {
            var item = TestStrings[i];
            Assert.That(() => trie.Add(item), Is.True);
            Assert.That(() => trie.HowManyStartsWithPrefix(item), Is.EqualTo(1));
        }

        for (int i = 0; i < length; i++)
        {
            var item = TestStrings[i];
            Assert.That(() => trie.HowManyStartsWithPrefix(item), Is.EqualTo(length - i));
        }

        Assert.That(() => trie.HowManyStartsWithPrefix("random_prefix"u8), Is.Zero);
    }
}
