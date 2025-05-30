// <copyright file="CalcErrorTests.cs" company="Ilya Krivtsov">
// Copyright (c) Ilya Krivtsov. All rights reserved.
// </copyright>

namespace Calc.Tests;

public class CalcErrorTests
{
    private Calc calc;

    [SetUp]
    public void Setup()
    {
        calc = new();
    }

    [Test]
    public void Test_0_Divide_0_Gives_Error()
    {
        calc.EnterDigit(0);
        calc.OperatorPressed(ArithmeticOperation.Division);
        calc.EnterDigit(0);
        calc.EqualsPressed();
        Assert.That(calc.DisplayResult, Is.EqualTo("Error"));
    }
}
