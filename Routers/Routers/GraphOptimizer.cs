namespace Routers;

/// <summary>
/// Utility class for optimizing network graphs.
/// </summary>
public static class GraphOptimizer
{
    /// <summary>
    /// Optimizes network graph.
    /// </summary>
    /// <param name="graph">Graph to optimize.</param>
    /// <returns>Optimized graph.</returns>
    public static Graph Optimize(Graph graph)
    {
        // use negative weights as workaround, so +1 is greater than any broadband
        var broadbands = graph.Nodes.ToDictionary(x => x, x => 1);
        var visited = new HashSet<Graph.Node>();

        var queue = new PriorityQueue<Edge, int>();
        queue.Enqueue(new(graph.Nodes[0], graph.Nodes[0]), 0);

        broadbands[graph.Nodes[0]] = 0;

        var newNodes = new List<Graph.Node>();
        var oldToNewMap = new Dictionary<Graph.Node, Graph.Node>();
        while (queue.TryDequeue(out Edge edge, out int broadband))
        {
            var node = edge.To;
            visited.Add(node);

            var newNode = new Graph.Node([]);
            newNodes.Add(newNode);
            oldToNewMap[node] = newNode;
            if (edge.From != edge.To)
            {
                oldToNewMap[edge.From].Neighbors[edge.To] = -broadband;
            }

            foreach (var (neighbor, neighborBroadband) in node.Neighbors)
            {
                if (visited.Contains(neighbor))
                {
                    continue;
                }

                if (neighborBroadband > broadbands[neighbor])
                {
                    broadbands[neighbor] = neighborBroadband;
                    queue.Enqueue(new(node, neighbor), -neighborBroadband);
                }
            }
        }

        if (visited.Count != graph.Nodes.Count)
        {
            throw new ArgumentException("The graph is disconnected", nameof(graph));
        }

        return new(newNodes);
    }

    private readonly record struct Edge(Graph.Node From, Graph.Node To);
}
