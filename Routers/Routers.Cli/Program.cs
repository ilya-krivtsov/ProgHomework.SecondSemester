using Routers;

if (args.Length < 2)
{
    Console.WriteLine("""
    Network topology optimizer
    Usage:
        dotnet run -- {inputFile} {outputFile} [-f|--force]

    Arguments:
        inputFile       file path to read network topology from
        outputFile      file path to write optimized network topology to
        -f or --force   overwrite outputFile if it already exists
    """);
    return 0;
}

var inputFilePath = args[0];
var outputFilePath = args[1];

if (!File.Exists(inputFilePath))
{
    Console.Error.WriteLine($"error: input file '{inputFilePath}' does not exist");
    return 1;
}

var force = args.Length >= 3 && args[2] is "-f" or "--force";

if (!force && File.Exists(outputFilePath))
{
    Console.Write($"File '{outputFilePath}' already exists, overwrite? (y/n): ");
    if (Console.ReadLine()?.Trim() != "y")
    {
        Console.WriteLine("Cancelled");
        return 0;
    }
}

Graph graph;
using (var inputFile = File.OpenText(inputFilePath))
{
    try
    {
        graph = Graph.ReadGraph(inputFile);
    }
    catch (InvalidDataException)
    {
        Console.Error.WriteLine($"error: invalid input file format");
        return 1;
    }
}

if (!GraphOptimizer.Optimize(graph, out var optimized))
{
    Console.Error.WriteLine($"error: graph is disconected");
    return 1;
}

using var outputFile = File.CreateText(outputFilePath);
optimized.Write(outputFile);

return 0;
