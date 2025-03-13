namespace Zipper.Tests.Streams;

using System.Diagnostics.CodeAnalysis;

[SetUpFixture]
[ExcludeFromCodeCoverage]
public class StreamTestsSource
{
    // "https://filesamples.com/samples/image/bmp/sample_640×426.bmp";
    private const string ImageDataUrl = "https://filesamples.com/samples/image/bmp/sample_640%C3%97426.bmp";

    // "https://neerc.ifmo.ru/wiki/index.php?title=Алгоритм_LZW";
    private const string TextDataUrl = "https://neerc.ifmo.ru/wiki/index.php?title=%D0%90%D0%BB%D0%B3%D0%BE%D1%80%D0%B8%D1%82%D0%BC_LZW";

    public static byte[] TextData { get; private set; }

    public static byte[] ImageData { get; private set; }

    [OneTimeSetUp]
    public static void DataSetup()
    {
        TextData = DownloadOrReuse(TextDataUrl);
        ImageData = DownloadOrReuse(ImageDataUrl);
    }

    private static byte[] DownloadOrReuse(string url)
    {
        string testFilesDirectory = "test_files";
        if (!Directory.Exists(testFilesDirectory))
        {
            Directory.CreateDirectory(testFilesDirectory);
        }

        var filename = $"test_file_{string.Concat(url.Select(c => char.IsAsciiLetterOrDigit(c) ? c : '_'))}";
        var filePath = Path.Combine(testFilesDirectory, filename);

        if (File.Exists(filePath))
        {
            return File.ReadAllBytes(filePath);
        }

        using var client = new HttpClient();
        using var memory = new MemoryStream();

        var response = client.Send(new HttpRequestMessage(HttpMethod.Get, url));
        response.EnsureSuccessStatusCode();

        using var content = response.Content.ReadAsStream();
        content.CopyTo(memory);

        var data = memory.ToArray();
        File.WriteAllBytes(filePath, data);

        return data;
    }
}
