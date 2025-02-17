namespace BurrowsWheelerTransform;

using System.Diagnostics;

/// <summary>
/// Burrows-Wheeler transform implementation.
/// </summary>
public static class Transform
{
    /// <summary>
    /// Transforms given string using Burrows-Wheeler algorithm.
    /// </summary>
    /// <param name="input">Input string.</param>
    /// <returns>Result of transformation.</returns>
    public static TransformResult ForwardTransform(string input)
    {
        int length = input.Length;

        if (length == 0)
        {
            return new(string.Empty, -1);
        }

        Span<int> offsets = stackalloc int[length];
        for (int i = 0; i < length; i++)
        {
            offsets[i] = i;
        }

        int Compare(int x, int y)
        {
            for (int i = 0; i < length; i++)
            {
                int compare = input[(i + x) % length] - input[(i + y) % length];
                if (compare != 0)
                {
                    return compare;
                }
            }

            return 0;
        }

        offsets.Sort(Compare);

        int? identityPosition = null;
        Span<char> result = stackalloc char[length];
        for (int i = 0; i < length; i++)
        {
            if (offsets[i] == 0)
            {
                identityPosition = i;
            }

            result[i] = input[(offsets[i] + length - 1) % length];
        }

        Debug.Assert(identityPosition.HasValue, "Identity position not found");

        return new(new(result), identityPosition.Value);
    }

    /// <summary>
    /// Reconstructs string transformed with Burrows-Wheeler algorithm.
    /// </summary>
    /// <param name="result">Transformed string.</param>
    /// <returns>Reconstructed string.</returns>
    public static string InverseTransform(TransformResult result)
    {
        if (result.IdentityIndex == -1)
        {
            return string.Empty;
        }

        int length = result.Value.Length;

        Span<int> appearances = stackalloc int[length];
        var lastAppearances = new Dictionary<char, int>();
        var charCounter = new SortedDictionary<char, int>();

        for (int i = 0; i < length; i++)
        {
            char currentChar = result.Value[i];

            if (!charCounter.TryGetValue(currentChar, out int count))
            {
                charCounter[currentChar] = 0;
            }

            charCounter[currentChar]++;

            appearances[i] =
                lastAppearances.TryGetValue(currentChar, out int lastIndex)
                ? appearances[lastIndex] + 1
                : 0;

            lastAppearances[currentChar] = i;
        }

        var lesserCharsCounter = new Dictionary<char, int>();
        int previousCount = 0;
        foreach (var (character, count) in charCounter)
        {
            lesserCharsCounter[character] = previousCount;
            previousCount += count;
        }

        Span<char> reconstructed = stackalloc char[length];

        int lastIdentityIndex = result.IdentityIndex;
        char lastCharacter = result.Value[lastIdentityIndex];
        reconstructed[^1] = result.Value[result.IdentityIndex];

        for (int i = 1; i < length; i++)
        {
            lastIdentityIndex = appearances[lastIdentityIndex] + lesserCharsCounter[lastCharacter];
            lastCharacter = result.Value[lastIdentityIndex];
            reconstructed[^(i + 1)] = lastCharacter;
        }

        return new(reconstructed);
    }
}
