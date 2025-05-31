namespace Zipper.Tests;

public static class TestUtil
{
    public static IEnumerable<byte[]> GetRandomStrings()
    {
        int seed = 74687324;
        var random = new Random(seed);

        int steps = 16;
        int length = 256;

        for (int i = 0; i < steps; i++)
        {
            var buffer = new byte[length];
            random.NextBytes(buffer);
            yield return buffer;
        }
    }
}
