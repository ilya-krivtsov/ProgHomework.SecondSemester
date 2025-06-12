// <copyright file="MyLinq.cs" company="Ilya Krivtsov">
// Copyright (c) Ilya Krivtsov. All rights reserved.
// </copyright>

namespace MyLinq;

/// <summary>
/// Utility class with GetPrimes, Take and Skip functions.
/// </summary>
public static class MyLinq
{
    /// <summary>
    /// Practically infinite sequence of prime numbers.
    /// </summary>
    /// <returns>Sequence of prime numbers.</returns>
    public static IEnumerable<ulong> GetPrimes()
    {
        yield return 2;
        yield return 3;
        ulong lastPrime = 3;
        while (true)
        {
            lastPrime += 2;
            bool isPrime = true;
            for (ulong i = 2; i * i <= lastPrime; i++)
            {
                if (lastPrime % i == 0)
                {
                    isPrime = false;
                    break;
                }
            }

            if (isPrime)
            {
                yield return lastPrime;
            }
        }
    }

    /// <summary>
    /// Sequence with first <paramref name="count"/> items from <paramref name="sequence"/>.
    /// </summary>
    /// <typeparam name="T">Item type.</typeparam>
    /// <param name="sequence">Sequence to take first <paramref name="count"/> items from.</param>
    /// <param name="count">How many items to take.</param>
    /// <returns>Subsequence of <paramref name="sequence"/>.</returns>
    public static IEnumerable<T> Take<T>(this IEnumerable<T> sequence, int count)
    {
        int index = 0;
        foreach (var item in sequence)
        {
            if (index >= count)
            {
                yield break;
            }

            yield return item;
            index++;
        }
    }

    /// <summary>
    /// Sequence without first <paramref name="count"/> items from <paramref name="sequence"/>.
    /// </summary>
    /// <typeparam name="T">Item type.</typeparam>
    /// <param name="sequence">Sequence to skip first <paramref name="count"/> items from.</param>
    /// <param name="count">How many items to skip.</param>
    /// <returns>Subsequence of <paramref name="sequence"/>.</returns>
    public static IEnumerable<T> Skip<T>(this IEnumerable<T> sequence, int count)
    {
        int index = 0;
        foreach (var item in sequence)
        {
            if (index >= count)
            {
                yield return item;
            }

            index++;
        }
    }
}
