namespace Trie;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Trie data structure, also known as prefix tree.
/// </summary>
public class Trie
{
    private readonly Node rootNode = new();

    /// <summary>
    /// Gets count of all strings stored in this trie.
    /// </summary>
    public int Size => rootNode.TotalDescendants;

    /// <summary>
    /// Adds <paramref name="item"/> to this trie.
    /// </summary>
    /// <param name="item">The string to add.</param>
    /// <returns><see langword="true"/> if <paramref name="item"/> wasn't present in trie before adding it, <see langword="false"/> otherwise.</returns>
    public bool Add(string item)
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
    /// asa.
    /// </summary>
    /// <param name="item">The string to seek.</param>
    /// <returns><see langword="true"/> if <paramref name="item"/> is present in trie, <see langword="false"/> otherwise.</returns>
    public bool Contains(string item)
        => GetNode(item, out var node) && node.EndOfWord;

    /// <summary>
    /// Removes <paramref name="item"/> from this trie.
    /// </summary>
    /// <param name="item">The string to remove.</param>
    /// <returns><see langword="true"/> if <paramref name="item"/> was present in trie before removing it, <see langword="false"/> otherwise.</returns>
    public bool Remove(string item)
    {
        if (!GetNode(item, out var node) || node.EndOfWord == false)
        {
            return false;
        }

        node.RemoveSelf();

        return true;
    }

    /// <summary>
    /// Gets count of strings stored in this trie that start with <paramref name="prefix"/>.
    /// </summary>
    /// <param name="prefix">Prefix to check against.</param>
    /// <returns>Count of strings stored in this trie that start with <paramref name="prefix"/>.</returns>
    public int HowManyStartsWithPrefix(ReadOnlySpan<char> prefix)
        => GetNode(prefix, out var node) ? node.TotalDescendants : 0;

    private bool GetNode(ReadOnlySpan<char> prefix, [MaybeNullWhen(false)] out Node node)
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
        private readonly Dictionary<char, Node> children = [];
        private readonly Node? parent;
        private readonly char character;

        internal Node(Node? parent = null, char character = '\0')
        {
            this.parent = parent;
            this.character = character;
        }

        public bool EndOfWord { get; private set; }

        // node itself is counted as descendant if marked as end of word
        public int TotalDescendants { get; set; }

        public Node GetOrCreateChild(char character)
        {
            if (!children.TryGetValue(character, out Node? node))
            {
                node = new(this, character);
                children[character] = node;

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
            var lastCharacter = character;
            while (lastNode.parent != null && lastNode.TotalDescendants == 1)
            {
                lastCharacter = lastNode.character;
                lastNode = lastNode.parent;
            }

            lastNode.children.Remove(lastCharacter);

            while (lastNode != null)
            {
                lastNode.TotalDescendants--;
                lastNode = lastNode.parent;
            }
        }

        public bool TryGetChild(char character, [MaybeNullWhen(false)] out Node node) => children.TryGetValue(character, out node);

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
