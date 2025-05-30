// <copyright file="Functions.cs" company="Ilya Krivtsov">
// Copyright (c) Ilya Krivtsov. All rights reserved.
// </copyright>

namespace Functions;

/// <summary>
/// Utility class that contains Map, Filter and Fold functions.
/// </summary>
public static class Functions
{
    /// <summary>
    /// Maps all elements from <paramref name="source"/> to elements of type <typeparamref name="TResult"/> using <paramref name="map"/> function.
    /// </summary>
    /// <typeparam name="TSource">Source type.</typeparam>
    /// <typeparam name="TResult">Type to map to.</typeparam>
    /// <param name="source">Source to map.</param>
    /// <param name="map">Function that maps <typeparamref name="TSource"/> to <typeparamref name="TResult"/>.</param>
    /// <returns>Sequence of mapped elements.</returns>
    public static IEnumerable<TResult> Map<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> map)
    {
        foreach (var item in source)
        {
            yield return map(item);
        }
    }

    /// <summary>
    /// Filters elements from <paramref name="source"/> that satisfy <paramref name="predicate"/>.
    /// </summary>
    /// <typeparam name="T">Type of elements.</typeparam>
    /// <param name="source">Source to filter.</param>
    /// <param name="predicate">Function that filters elements; if it returns <see langword="true"/>, adds element to the result, otherwise, doesn't.</param>
    /// <returns>Sequence of filtered elements.</returns>
    public static IEnumerable<T> Filter<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        foreach (var item in source)
        {
            if (predicate(item))
            {
                yield return item;
            }
        }
    }

    /// <summary>
    /// Folds all elements in <paramref name="source"/> into one element.
    /// </summary>
    /// <typeparam name="T">Type of elements.</typeparam>
    /// <param name="source">Source to fold.</param>
    /// <param name="folder">Function that is used to accumulate result.</param>
    /// <returns>Accumulated value.</returns>
    /// <exception cref="InvalidOperationException">Sequence is empty.</exception>
    public static T Fold<T>(this IEnumerable<T> source, Func<T, T, T> folder)
    {
        var enumerator = source.GetEnumerator();

        if (!enumerator.MoveNext())
        {
            throw new InvalidOperationException("No elements in source");
        }

        var value = enumerator.Current;

        while (enumerator.MoveNext())
        {
            value = folder(value, enumerator.Current);
        }

        return value;
    }

    /// <summary>
    /// Folds all elements in <paramref name="source"/> into one element.
    /// </summary>
    /// <typeparam name="TSource">Type of elements in <paramref name="source"/>.</typeparam>
    /// <typeparam name="TResult">The type of accumulated value.</typeparam>
    /// <param name="source">Source to fold.</param>
    /// <param name="initialValue">Initial value of accumulated value.</param>
    /// <param name="folder">Function that is used to accumulate result.</param>
    /// <returns>Accumulated value.</returns>
    public static TResult Fold<TSource, TResult>(this IEnumerable<TSource> source, TResult initialValue, Func<TResult, TSource, TResult> folder)
    {
        var value = initialValue;
        foreach (var item in source)
        {
            value = folder(value, item);
        }

        return value;
    }

    /// <summary>
    /// Folds all elements in <paramref name="source"/> into one element.
    /// </summary>
    /// <typeparam name="TSource">Type of elements in <paramref name="source"/>.</typeparam>
    /// <typeparam name="TIntermediate">The type of accumulated value.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="source">Source to fold.</param>
    /// <param name="initialValue">Initial value of accumulated value.</param>
    /// <param name="folder">Function that is used to accumulate result.</param>
    /// <param name="map">Function that is used to convert accumulated result.</param>
    /// <returns>Converted accumulated value.</returns>
    public static TResult Fold<TSource, TIntermediate, TResult>(this IEnumerable<TSource> source, TIntermediate initialValue, Func<TIntermediate, TSource, TIntermediate> folder, Func<TIntermediate, TResult> map)
    {
        var value = initialValue;
        foreach (var item in source)
        {
            value = folder(value, item);
        }

        return map(value);
    }
}
