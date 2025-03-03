namespace Zipper;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Trie data structure, also known as prefix tree, implemented as dictionary.
/// </summary>
public class Trie
{
    private readonly Node rootNode = new();

    /// <summary>
    /// Adds <paramref name="key"/> associated with <paramref name="value"/> to this trie.
    /// </summary>
    /// <param name="key">The byte sequence to as key.</param>
    /// <param name="value">The number to add as value.</param>
    /// <returns><see langword="true"/> if <paramref name="key"/> wasn't present in trie before adding it, <see langword="false"/> otherwise.</returns>
    public bool Add(ReadOnlySpan<byte> key, int value)
    {
        var lastNode = rootNode;
        foreach (var character in key)
        {
            lastNode = lastNode.GetOrCreateChild(character);
        }

        if (lastNode.EndOfWord)
        {
            return false;
        }

        lastNode.EndOfWord = true;
        lastNode.Value = value;

        return true;
    }

    /// <summary>
    /// Tries to get value associated with <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="value">
    /// When this method returns, contains the value associated with <paramref name="key"/>, if <paramref name="key"/> is found, zero otherwise.
    /// </param>
    /// <returns><see langword="true"/> if <paramref name="key"/> is found, <see langword="false"/> otherwise.</returns>
    public bool TryGetValue(ReadOnlySpan<byte> key, out int value)
    {
        var nodeExistsAndHasValue = GetNode(key, out var node) && node.EndOfWord;

        value = node?.Value ?? 0;

        return nodeExistsAndHasValue;
    }

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

        public bool EndOfWord { get; set; }

        public int Value { get; set; }

        public Node GetOrCreateChild(byte value)
        {
            if (!children.TryGetValue(value, out Node? node))
            {
                node = new();
                children[value] = node;

                return node;
            }

            return node;
        }

        public bool TryGetChild(byte value, [MaybeNullWhen(false)] out Node node)
            => children.TryGetValue(value, out node);
    }
}
