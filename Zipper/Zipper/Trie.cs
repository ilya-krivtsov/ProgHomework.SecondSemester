namespace Zipper;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Trie data structure, also known as prefix tree.
/// </summary>
public class Trie
{
    private readonly Node rootNode = new(null, 0);

    /// <summary>
    /// Gets count of all strings stored in this trie.
    /// </summary>
    public int Size => rootNode.TotalDescendants;

    /// <summary>
    /// Adds <paramref name="item"/> to this trie.
    /// </summary>
    /// <param name="item">The byte sequence to add.</param>
    /// <returns><see langword="true"/> if <paramref name="item"/> wasn't present in trie before adding it, <see langword="false"/> otherwise.</returns>
    public bool Add(ReadOnlySpan<byte> item)
    {
        var lastNode = rootNode;
        foreach (var character in item)
        {
            lastNode = lastNode.GetOrCreateChild(character);
        }

        if (lastNode.EndOfWord)
        {
            return false;
        }

        lastNode.MarkAsEndOfWord();

        return true;
    }

    /// <summary>
    /// Checks if this trie contains <paramref name="item"/>.
    /// </summary>
    /// <param name="item">The byte sequence to seek.</param>
    /// <returns><see langword="true"/> if <paramref name="item"/> is present in trie, <see langword="false"/> otherwise.</returns>
    public bool Contains(ReadOnlySpan<byte> item)
        => GetNode(item, out var node) && node.EndOfWord;

    /// <summary>
    /// Removes <paramref name="item"/> from this trie.
    /// </summary>
    /// <param name="item">The byte sequence to remove.</param>
    /// <returns><see langword="true"/> if <paramref name="item"/> was present in trie before removing it, <see langword="false"/> otherwise.</returns>
    public bool Remove(ReadOnlySpan<byte> item)
    {
        if (!GetNode(item, out var node) || node.EndOfWord == false)
        {
            return false;
        }

        node.RemoveSelf();

        return true;
    }

    /// <summary>
    /// Gets count of byte sequences stored in this trie that start with <paramref name="prefix"/>.
    /// </summary>
    /// <param name="prefix">Prefix to check against.</param>
    /// <returns>Count of byte sequences stored in this trie that start with <paramref name="prefix"/>.</returns>
    public int HowManyStartsWithPrefix(ReadOnlySpan<byte> prefix)
        => GetNode(prefix, out var node) ? node.TotalDescendants : 0;

    private bool GetNode(ReadOnlySpan<byte> prefix, [MaybeNullWhen(false)] out Node node)
    {
        node = rootNode;
        foreach (var character in prefix)
        {
            if (!node.TryGetChild(character, out node))
            {
                return false;
            }
        }

        return true;
    }

    private class Node
    {
        private readonly Dictionary<byte, Node> children = [];
        private readonly Node? parent;
        private readonly byte value;

        internal Node(Node? parent, byte value)
        {
            this.parent = parent;
            this.value = value;
        }

        public bool EndOfWord { get; private set; }

        // node itself is counted as descendant if marked as end of word
        public int TotalDescendants { get; set; }

        public Node GetOrCreateChild(byte value)
        {
            if (!children.TryGetValue(value, out Node? node))
            {
                node = new(this, value);
                children[value] = node;

                return node;
            }

            return node;
        }

        public void RemoveSelf()
        {
            EndOfWord = false;
            TotalDescendants--;

            if (parent == null)
            {
                return;
            }

            var lastNode = parent;
            var lastValue = value;
            while (lastNode.parent != null && lastNode.TotalDescendants == 1)
            {
                lastValue = lastNode.value;
                lastNode = lastNode.parent;
            }

            lastNode.children.Remove(lastValue);

            while (lastNode != null)
            {
                lastNode.TotalDescendants--;
                lastNode = lastNode.parent;
            }
        }

        public bool TryGetChild(byte value, [MaybeNullWhen(false)] out Node node) => children.TryGetValue(value, out node);

        public void MarkAsEndOfWord()
        {
            EndOfWord = true;
            TotalDescendants++;

            var lastNode = parent;
            while (lastNode != null)
            {
                lastNode.TotalDescendants++;
                lastNode = lastNode.parent;
            }
        }
    }
}
