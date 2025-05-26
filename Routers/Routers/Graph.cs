namespace Routers;

using System.Text.RegularExpressions;

/// <summary>
/// Network graph.
/// </summary>
public partial class Graph
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Graph"/> class.
    /// </summary>
    /// <param name="nodes">Nodes of the graph.</param>
    internal Graph(IEnumerable<Node> nodes)
    {
        Nodes = [.. nodes];
    }

    /// <summary>
    /// Gets all nodes in graph.
    /// </summary>
    internal List<Node> Nodes { get; }

    /// <summary>
    /// Reads graph from <paramref name="reader"/>.
    /// </summary>
    /// <param name="reader"><see cref="TextReader"/> to read graph from.</param>
    /// <returns>Read graph.</returns>
    public static Graph ReadGraph(TextReader reader)
    {
        var graph = new Graph([]);
        var nodes = new Dictionary<int, Node>();

        while (true)
        {
            var line = reader.ReadLine();
            if (line == null)
            {
                break;
            }

            var nodeMatch = NodeRegex().Match(line);
            if (!nodeMatch.Success || nodeMatch.Groups.Count != 3 ||
                !int.TryParse(nodeMatch.Groups[1].ValueSpan, out int nodeIndex))
            {
                throw new InvalidDataException();
            }

            if (!nodes.TryGetValue(nodeIndex, out var node))
            {
                node = new([]);
                nodes[nodeIndex] = node;
                graph.Nodes.Add(node);
            }

            var rawConnections = nodeMatch.Groups[2].Value.Split(',');
            var connections = new Dictionary<int, int>();
            foreach (var connection in rawConnections)
            {
                var connectionMatch = ConnectionRegex().Match(connection);
                if (!connectionMatch.Success || connectionMatch.Groups.Count != 3)
                {
                    throw new InvalidDataException();
                }

                if (!int.TryParse(connectionMatch.Groups[1].ValueSpan, out int neighborIndex) ||
                    !int.TryParse(connectionMatch.Groups[2].ValueSpan, out int bandwidth) ||
                    connections.ContainsKey(neighborIndex))
                {
                    throw new InvalidDataException();
                }

                connections[neighborIndex] = bandwidth;
            }

            foreach (var (neighborIndex, bandwidth) in connections)
            {
                if (!nodes.TryGetValue(neighborIndex, out var neighbor))
                {
                    neighbor = new([]);
                    nodes[neighborIndex] = neighbor;
                    graph.Nodes.Add(neighbor);
                }

                node.Neighbors[neighbor] = bandwidth;
                neighbor.Neighbors[node] = bandwidth;
            }
        }

        return graph;
    }

    /// <summary>
    /// Writes graph to <paramref name="writer"/>.
    /// </summary>
    /// <param name="writer"><see cref="TextWriter"/> to write graph to.</param>
    public void Write(TextWriter writer)
    {
        var wroteConnections = new HashSet<(Node, Node)>();
        var nodeIndexLookup = new Dictionary<Node, int>();
        int lastIndex = 0;

        int GetIndex(Node node)
        {
            if (!nodeIndexLookup.TryGetValue(node, out int index))
            {
                nodeIndexLookup[node] = lastIndex;
                index = lastIndex;
                lastIndex++;
            }

            return index;
        }

        foreach (var node in Nodes)
        {
            int neighborCount = 0;
            foreach (var (neighbor, bandwidth) in node.Neighbors)
            {
                if (wroteConnections.Contains((node, neighbor)) ||
                    wroteConnections.Contains((neighbor, node)))
                {
                    continue;
                }

                wroteConnections.Add((node, neighbor));
                wroteConnections.Add((neighbor, node));

                if (neighborCount == 0)
                {
                    writer.Write($"{GetIndex(node)}: ");
                }
                else if (neighborCount == 1)
                {
                    writer.Write(", ");
                }

                writer.Write($"{GetIndex(neighbor)} ({bandwidth})");
                neighborCount++;
            }

            if (neighborCount != 0)
            {
                writer.WriteLine();
            }
        }
    }

    [GeneratedRegex(@"^\s*(\d+)\s*:(.*)$")]
    private static partial Regex NodeRegex();

    [GeneratedRegex(@"\s*(\d+)\s*\(\s*(\d+)\s*\)\s*")]
    private static partial Regex ConnectionRegex();

    /// <summary>
    /// Graph node.
    /// </summary>
    /// <param name="Neighbors">Neighbors of node, stored as pairs of <see cref="Node"/> and bandwidth.</param>
    internal record Node(Dictionary<Node, int> Neighbors);
}
