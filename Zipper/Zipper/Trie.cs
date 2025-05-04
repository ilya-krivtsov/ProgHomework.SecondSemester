namespace Zipper;

/// <summary>
/// Trie data structure, also known as prefix tree, that can be traversed through.
/// </summary>
/// <typeparam name="T">Type of values.</typeparam>
internal class Trie<T>
    where T : struct
{
    private readonly Node rootNode = new(default);
    private Node lastNode;

    /// <summary>
    /// Initializes a new instance of the <see cref="Trie{T}"/> class.
    /// </summary>
    public Trie()
    {
        lastNode = rootNode;
    }

    /// <summary>
    /// Gets value stored at current position in trie.
    /// </summary>
    public T CurrentValue => lastNode.Value;

    /// <summary>
    /// Gets a value indicating whether current position is root.
    /// </summary>
    public bool AtRoot => lastNode == rootNode;

    /// <summary>
    /// Resets position to root.
    /// </summary>
    public void Reset()
    {
        lastNode = rootNode;
    }

    /// <summary>
    /// Adds child at current position with specified <paramref name="key"/> and <paramref name="value"/>.
    /// </summary>
    /// <param name="key">Key of child to add.</param>
    /// <param name="value">Value of child to add.</param>
    /// <returns><see langword="true"/> if child with specified key did not exist at current position, <see langword="false"/> otherwise.</returns>
    public bool AddChild(byte key, T value)
    {
        if (HasChild(key))
        {
            return false;
        }

        var node = new Node(value);
        lastNode.Children[key] = node;

        return true;
    }

    /// <summary>
    /// Moves forward if <paramref name="key"/> is found, otherwise doesn't move.
    /// </summary>
    /// <param name="key">Key to search for.</param>
    /// <returns><see langword="true"/> if moved forward, <see langword="false"/> otherwise.</returns>
    public bool MoveForward(byte key)
    {
        if (lastNode.Children.TryGetValue(key, out var existingNode))
        {
            lastNode = existingNode;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if child with specified <paramref name="key"/> exists at current position.
    /// </summary>
    /// <param name="key">Key to search for.</param>
    /// <returns><see langword="true"/> if child with specified key did not exist at current position, <see langword="false"/> otherwise.</returns>
    public bool HasChild(byte key)
        => lastNode.Children.ContainsKey(key);

    private class Node(T value)
    {
        public Dictionary<byte, Node> Children { get; } = [];

        public T Value { get; } = value;
    }
}
