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

    private readonly byte testKey = 157;
    private readonly int testValue = 252354;

    private Trie<int> trie;

    [SetUp]
    public void Setup()
    {
        trie = new();
    }

    [Test]
    public void AddChild_ShouldReturnTrue_IfChildDidNotExists()
    {
        Assert.Multiple(() =>
        {
            Assert.That(trie.HasChild(testKey), Is.False);
            Assert.That(trie.AddChild(testKey, testValue), Is.True);
        });
    }

    [Test]
    public void HasChild_ShouldReturnTrue_IfAddedChild()
    {
        trie.AddChild(testKey, testValue);
        Assert.That(trie.HasChild(testKey), Is.True);
    }

    [Test]
    public void HasChild_ShouldReturnFalse_IfChildDoesNotExist()
    {
        Assert.That(trie.HasChild(testKey), Is.False);
    }

    [Test]
    public void AddChild_ShouldReturnFalse_IfChildExisted()
    {
        trie.AddChild(testKey, testValue);
        Assert.That(trie.AddChild(testKey, testValue), Is.False);
    }

    [Test]
    public void Add_ShouldNotMove()
    {
        Assert.Multiple(() =>
        {
            Assert.That(trie.AtRoot, Is.True);
            Assert.That(trie.AddChild(testKey, testValue), Is.True);
            Assert.That(trie.AtRoot, Is.True);
        });
    }

    [Test]
    public void MoveForward_ShouldReturnTrue_IfMovingToAddedChild()
    {
        trie.AddChild(testKey, testValue);
        Assert.That(trie.MoveForward(testKey), Is.True);
    }

    [Test]
    public void MoveForward_ShouldReturnFalse_IfChildDoesNotExist()
    {
        Assert.That(trie.MoveForward(testKey), Is.False);
    }

    [Test]
    public void MoveForward_ShouldMove()
    {
        trie.AddChild(testKey, testValue);
        Assert.Multiple(() =>
        {
            Assert.That(trie.AtRoot, Is.True);
            Assert.That(trie.MoveForward(testKey), Is.True);
            Assert.That(trie.AtRoot, Is.False);
        });
    }

    [Test]
    public void Reset_ShouldReset_IfMoved()
    {
        Assert.That(trie.AtRoot, Is.True);

        trie.AddChild(testKey, testValue);
        trie.MoveForward(testKey);
        Assert.That(trie.AtRoot, Is.False);

        trie.Reset();
        Assert.That(trie.AtRoot, Is.True);
    }

    [Test]
    public void AddChild_ShouldAdd_Once()
    {
        int valueA = 3463235;
        int valueB = 73334536;

        trie.AddChild(testKey, valueA);
        Assert.That(trie.AddChild(testKey, valueB), Is.False);
    }

    [Test]
    public void CurrentValue_ShouldReturnAddedValue()
    {
        trie.AddChild(testKey, testValue);
        trie.MoveForward(testKey);
        Assert.That(trie.CurrentValue, Is.EqualTo(testValue));
    }
}
