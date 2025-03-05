namespace Zipper;

using System.Buffers;
using System.Diagnostics;

/// <summary>
/// Burrows-Wheeler transform implementation.
/// </summary>
internal static class BWT
{
    private static readonly ArrayPool<int> Pool = ArrayPool<int>.Create();

    /// <summary>
    /// Transforms given byte sequence using Burrows-Wheeler algorithm.
    /// </summary>
    /// <param name="input">Input byte sequence.</param>
    /// <param name="output">Span to write transofrmed input to.</param>
    /// <returns>Index that is used to reconstruct byte sequence.</returns>
    public static int ForwardTransform(Memory<byte> input, Span<byte> output)
    {
        Debug.Assert(input.Length == output.Length, "Length of input and output should be the same");
        int length = input.Length;

        if (length == 0)
        {
            return -1;
        }

        int[] offsets = Pool.Rent(length);
        for (int i = 0; i < length; i++)
        {
            offsets[i] = i;
        }

        int Compare(int x, int y)
        {
            var inputSpan = input.Span;
            for (int i = 0; i < length; i++)
            {
                int compare = inputSpan[(i + x) % length] - inputSpan[(i + y) % length];
                if (compare != 0)
                {
                    return compare;
                }
            }

            return 0;
        }

        var offsetsSpan = offsets.AsSpan(0, length);

        offsetsSpan.Sort(Compare);

        var inputSpan = input.Span;
        int? identityPosition = null;
        for (int i = 0; i < length; i++)
        {
            if (offsets[i] == 0)
            {
                identityPosition = i;
            }

            output[i] = inputSpan[(offsets[i] + length - 1) % length];
        }

        Pool.Return(offsets);

        Debug.Assert(identityPosition.HasValue, "Identity position not found");

        return identityPosition.Value;
    }

    /// <summary>
    /// Reconstructs byte sequence transformed with Burrows-Wheeler algorithm.
    /// </summary>
    /// <param name="input">Transformed byte sequence.</param>
    /// <param name="identityIndex">Index that is used to reconstruct byte sequence.</param>
    /// <param name="output">Span to write reconstructed byte sequence to.</param>
    public static void InverseTransform(Span<byte> input, int identityIndex, Span<byte> output)
    {
        Debug.Assert(input.Length == output.Length, "Length of input and output should be the same");

        if (identityIndex == -1)
        {
            return;
        }

        int length = input.Length;

        int[] appearances = Pool.Rent(length);
        Span<int> lastAppearances = stackalloc int[256];
        Span<int> byteCounter = stackalloc int[256];

        for (int i = 0; i < 256; i++)
        {
            lastAppearances[i] = -1;
        }

        for (int i = 0; i < length; i++)
        {
            byte currentByte = input[i];
            byteCounter[currentByte]++;

            int lastAppearance = lastAppearances[currentByte];
            appearances[i] = lastAppearance == -1 ? 0 : appearances[lastAppearance] + 1;
            lastAppearances[currentByte] = i;
        }

        Span<int> lesserBytesCounter = stackalloc int[256];
        int previousCount = 0;
        for (int i = 0; i < 256; i++)
        {
            if (byteCounter[i] == 0)
            {
                continue;
            }

            lesserBytesCounter[i] = previousCount;
            previousCount += byteCounter[i];
        }

        int lastIdentityIndex = identityIndex;
        byte lastByte = input[lastIdentityIndex];
        output[^1] = input[identityIndex];

        for (int i = 1; i < length; i++)
        {
            lastIdentityIndex = appearances[lastIdentityIndex] + lesserBytesCounter[lastByte];
            lastByte = input[lastIdentityIndex];
            output[^(i + 1)] = lastByte;
        }

        Pool.Return(appearances);
    }
}
