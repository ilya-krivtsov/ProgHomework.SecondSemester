// <copyright file="FunctionsTests.cs" company="Ilya Krivtsov">
// Copyright (c) Ilya Krivtsov. All rights reserved.
// </copyright>

namespace Functions.Tests;

public class FunctionsTests
{
    private static readonly IEnumerable<int> NumberSequence = Enumerable.Range(0, 120);

    [Test]
    public void Map_ConvertsNumbersToString_SameAs_Select()
    {
        AssertMapBehavesSameAsSelect(NumberSequence, x => x.ToString());
    }

    [Test]
    public void Map_ConvertsStringToNumbers_SameAs_Select()
    {
        AssertMapBehavesSameAsSelect(NumberSequence.Select(x => x.ToString()), int.Parse);
    }

    [Test]
    public void Filter_SelectsNumbers_SameAs_Where()
    {
        AssertFilerBehavesSameAsWhere(NumberSequence, x => x % 2 == 0);
    }

    [Test]
    public void Filter_SelectsStrings_SameAs_Where()
    {
        AssertFilerBehavesSameAsWhere(NumberSequence.Select(x => x.ToString()), x => x.Contains('4'));
    }

    [Test]
    public void Fold_SumsNumbers_SameAsAggregate()
    {
        static int Sum(int x, int y) => x + y;

        var aggregateResult = NumberSequence.Aggregate(Sum);
        var foldResult = NumberSequence.Fold(Sum);

        Assert.That(aggregateResult, Is.EqualTo(foldResult));
    }

    [Test]
    public void Fold_AddsCharacters_SameAsAggregate()
    {
        static string Sum(string x, char y) => x + y;

        var source = Enumerable.Range('0', '~' - '0').Select(x => (char)x);

        var aggregateResult = source.Aggregate(string.Empty, Sum);
        var foldResult = source.Fold(string.Empty, Sum);

        Assert.That(aggregateResult, Is.EqualTo(foldResult));
    }

    [Test]
    public void Fold_AddsCharactersAndReverses_SameAsAggregate()
    {
        static string Sum(string x, char y) => x + y;
        static string Reverse(string s) => string.Concat(s.Reverse());

        var source = Enumerable.Range('0', '~' - '0').Select(x => (char)x);

        var aggregateResult = source.Aggregate(string.Empty, Sum, Reverse);
        var foldResult = source.Fold(string.Empty, Sum, Reverse);

        Assert.That(aggregateResult, Is.EqualTo(foldResult));
    }

    [Test]
    public void Fold_Throws_OnEmptyCollection()
    {
        Assert.Throws<InvalidOperationException>(() => Enumerable.Range(0, 0).Fold((x, y) => x + y));
    }

    private static void AssertMapBehavesSameAsSelect<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, TResult> map)
    {
        var selectResult = source.Select(map);
        var mapResult = source.Map(map);

        Assert.That(selectResult.SequenceEqual(mapResult), Is.True);
    }

    private static void AssertFilerBehavesSameAsWhere<T>(IEnumerable<T> source, Func<T, bool> filter)
    {
        var whereResult = source.Where(filter);
        var filterResult = source.Filter(filter);

        Assert.That(whereResult.SequenceEqual(filterResult), Is.True);
    }
}
