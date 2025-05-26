// <copyright file="ArithmeticOperation.cs" company="Ilya Krivtsov">
// Copyright (c) Ilya Krivtsov. All rights reserved.
// </copyright>

namespace Calc;

/// <summary>
/// Arithmetic operation that can be performed in <see cref="Calc"/>.
/// </summary>
public enum ArithmeticOperation
{
    /// <summary>
    /// Addition operation.
    /// </summary>
    Addition,

    /// <summary>
    /// Subtraction operation.
    /// </summary>
    Subtraction,

    /// <summary>
    /// Multiplication operation.
    /// </summary>
    Multiplication,

    /// <summary>
    /// Division operation.
    /// </summary>
    Division,
}
