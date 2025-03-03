namespace Zipper;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Trie data structure, also known as prefix tree.
/// </summary>
public class Trie
{
    private readonly Node rootNode = new();

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

        lastNode.EndOfWord = true;

        return true;
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
