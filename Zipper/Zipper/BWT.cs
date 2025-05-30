namespace Zipper;

using System.Buffers;
using System.Diagnostics;

/// <summary>
/// Burrows-Wheeler transform implementation.
/// </summary>
internal static class BWT
{
    private static readonly ArrayPool<int> Pool = ArrayPool<int>.Create();
    private static readonly ArrayPool<byte> InputPool = ArrayPool<byte>.Create();

    /// <summary>
    /// Transforms given byte sequence using Burrows-Wheeler algorithm.
    /// </summary>
    /// <param name="input">Input byte sequence.</param>
    /// <param name="output">Span to write transofrmed input to.</param>
    /// <returns>Index that is used to reconstruct byte sequence.</returns>
    public static int ForwardTransform(ReadOnlySpan<byte> input, Span<byte> output)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(output.Length, input.Length, nameof(output));

        int length = input.Length;

        if (length == 0)
        {
            return 0;
        }

        int[] offsets = Pool.Rent(length);
        for (int i = 0; i < length; i++)
        {
            offsets[i] = i;
        }

        var inputCopy = InputPool.Rent(length);
        input.CopyTo(inputCopy);

        int Compare(int x, int y)
        {
            for (int i = 0; i < length; i++)
            {
                int compare = inputCopy[(i + x) % length] - inputCopy[(i + y) % length];
                if (compare != 0)
                {
                    return compare;
                }
            }

            return 0;
        }

        var offsetsSpan = offsets.AsSpan(0, length);

        offsetsSpan.Sort(Compare);

        int? identityPosition = null;
        for (int i = 0; i < length; i++)
        {
            if (offsets[i] == 0)
            {
                identityPosition = i;
            }

            output[i] = inputCopy[(offsets[i] + length - 1) % length];
        }

        Pool.Return(offsets);
        InputPool.Return(inputCopy);

        Debug.Assert(identityPosition.HasValue, "Identity position not found");

        return identityPosition.Value;
    }

    /// <summary>
    /// Reconstructs byte sequence transformed with Burrows-Wheeler algorithm.
    /// </summary>
    /// <param name="input">Transformed byte sequence.</param>
    /// <param name="identityIndex">Index that is used to reconstruct byte sequence.</param>
    /// <param name="output">Span to write reconstructed byte sequence to.</param>
    public static void InverseTransform(ReadOnlySpan<byte> input, int identityIndex, Span<byte> output)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(identityIndex, nameof(identityIndex));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(identityIndex, input.Length, nameof(identityIndex));

        ArgumentOutOfRangeException.ThrowIfLessThan(output.Length, input.Length, nameof(output));

        if (input.Length <= 1)
        {
            input.CopyTo(output);
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
        output[length - 1] = input[identityIndex];

        for (int i = 1; i < length; i++)
        {
            lastIdentityIndex = appearances[lastIdentityIndex] + lesserBytesCounter[lastByte];
            lastByte = input[lastIdentityIndex];
            output[length - (i + 1)] = lastByte;
        }

        Pool.Return(appearances);
    }
}
