// <copyright file="MyLinqTests.cs" company="Ilya Krivtsov">
// Copyright (c) Ilya Krivtsov. All rights reserved.
// </copyright>

namespace MyLinq.Tests;

public class MyLinqTests
{
    [Test]
    public void GetPrimes_Yields_PrimeNumbers()
    {
        int iterations = 100;

        foreach (var (index, prime) in MyLinq.GetPrimes().Index())
        {
            if (index >= iterations)
            {
                break;
            }

            for (uint i = 2; i < prime; i++)
            {
                Assert.That(prime % i, Is.Not.EqualTo(0));
            }
        }
    }

    [Test]
    public void Take_ReturnsSequence_With_First_N_Elements()
    {
        int takeCount = 47;
        Assert.That(
            MyLinq.Take(Enumerable.Range(0, 100), takeCount)
            .SequenceEqual(Enumerable.Range(0, takeCount)),
            Is.True);
    }

    [Test]
    public void Take_ReturnsSameSequence_If_CountIsGreaterThanOrEqualTo_SourceCount()
    {
        int count = 100;
        var range = Enumerable.Range(0, count);
        Assert.Multiple(() =>
        {
            Assert.That(MyLinq.Take(range, count).SequenceEqual(range), Is.True);
            Assert.That(MyLinq.Take(range, count + 1).SequenceEqual(range), Is.True);
        });
    }

    [Test]
    public void Take_ReturnsEmptySequence_If_CountIs_LessThanOrEqualTo_Zero()
    {
        var range = Enumerable.Range(0, 100);
        Assert.Multiple(() =>
        {
            Assert.That(MyLinq.Take(range, 0).Any(), Is.False);
            Assert.That(MyLinq.Take(range, -1).Any(), Is.False);
        });
    }

    [Test]
    public void Skip_ReturnsSequence_Without_First_N_Elements()
    {
        int takeCount = 53;
        Assert.That(
            MyLinq.Skip(Enumerable.Range(0, 100), takeCount)
            .SequenceEqual(Enumerable.Range(takeCount, 100 - takeCount)),
            Is.True);
    }

    [Test]
    public void Skip_ReturnsSameSequence_If_CountIsLessThanOrEqualTo_Zero()
    {
        int count = 100;
        var range = Enumerable.Range(0, count);
        Assert.Multiple(() =>
        {
            Assert.That(MyLinq.Skip(range, 0).SequenceEqual(range), Is.True);
            Assert.That(MyLinq.Skip(range, -1).SequenceEqual(range), Is.True);
        });
    }

    [Test]
    public void Skip_ReturnsEmptySequence_If_CountIsGreaterThanOrEqualTo_SourceCount()
    {
        int count = 100;
        var range = Enumerable.Range(0, count);
        Assert.Multiple(() =>
        {
            Assert.That(MyLinq.Skip(range, count).Any(), Is.False);
            Assert.That(MyLinq.Skip(range, count + 1).Any(), Is.False);
        });
    }
}
