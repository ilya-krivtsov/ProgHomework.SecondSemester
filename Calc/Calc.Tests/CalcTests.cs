// <copyright file="CalcTests.cs" company="Ilya Krivtsov">
// Copyright (c) Ilya Krivtsov. All rights reserved.
// </copyright>

namespace Calc.Tests;

public class CalcTests
{
    private Calc calc;

    [SetUp]
    public void Setup()
    {
        calc = new();
    }

    [Test]
    public void Test_2_Plus_3_Equals_5()
    {
        calc.EnterDigit(2);
        Assert.That(calc.DisplayResult, Is.EqualTo("2"));

        calc.OperatorPressed(ArithmeticOperation.Addition);
        Assert.That(calc.DisplayResult, Is.EqualTo("2"));

        calc.EnterDigit(3);
        Assert.That(calc.DisplayResult, Is.EqualTo("3"));

        calc.EqualsPressed();
        Assert.That(calc.DisplayResult, Is.EqualTo("5"));
    }

    [Test]
    public void Test_2_Plus_3_Minus_10_Equals_Minus_5()
    {
        calc.EnterDigit(2);
        Assert.That(calc.DisplayResult, Is.EqualTo("2"));

        calc.OperatorPressed(ArithmeticOperation.Addition);
        Assert.That(calc.DisplayResult, Is.EqualTo("2"));

        calc.EnterDigit(3);
        Assert.That(calc.DisplayResult, Is.EqualTo("3"));

        calc.OperatorPressed(ArithmeticOperation.Subtraction);
        Assert.That(calc.DisplayResult, Is.EqualTo("5"));

        calc.EnterDigit(1);
        Assert.That(calc.DisplayResult, Is.EqualTo("1"));
        calc.EnterDigit(0);
        Assert.That(calc.DisplayResult, Is.EqualTo("10"));

        calc.EqualsPressed();
        Assert.That(calc.DisplayResult, Is.EqualTo("-5"));
    }

    [Test]
    public void Test_0_Point_1_Plus_0_Point_2_Equals_0_Point_3()
    {
        calc.EnterDigit(0);
        Assert.That(calc.DisplayResult, Is.EqualTo("0"));
        calc.EnterDecimalPoint();
        Assert.That(calc.DisplayResult, Is.EqualTo("0."));
        calc.EnterDigit(1);
        Assert.That(calc.DisplayResult, Is.EqualTo("0.1"));

        calc.OperatorPressed(ArithmeticOperation.Addition);
        Assert.That(calc.DisplayResult, Is.EqualTo("0.1"));

        calc.EnterDigit(0);
        Assert.That(calc.DisplayResult, Is.EqualTo("0"));
        calc.EnterDecimalPoint();
        Assert.That(calc.DisplayResult, Is.EqualTo("0."));
        calc.EnterDigit(2);
        Assert.That(calc.DisplayResult, Is.EqualTo("0.2"));

        calc.EqualsPressed();
        Assert.That(calc.DisplayResult, Is.EqualTo("0.3"));
    }
}
