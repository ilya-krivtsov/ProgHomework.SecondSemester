namespace Zipper.Tests.LZW;

[SetUpFixture]
public class LZWTestsSource
{
    // "https://en.wikipedia.org/wiki/Lempel–Ziv–Welch";
    private const string DataUrl = "https://en.wikipedia.org/wiki/Lempel%E2%80%93Ziv%E2%80%93Welch";

    public static byte[] Data { get; private set; }

    [OneTimeSetUp]
    public static void DataSetup()
    {
        using var client = new HttpClient();
        var response = client.Send(new HttpRequestMessage(HttpMethod.Get, DataUrl));
        response.EnsureSuccessStatusCode();
        using var content = response.Content.ReadAsStream();
        using var memory = new MemoryStream();
        content.CopyTo(memory);
        Data = memory.ToArray();
    }
}
