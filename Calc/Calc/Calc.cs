// <copyright file="Calc.cs" company="Ilya Krivtsov">
// Copyright (c) Ilya Krivtsov. All rights reserved.
// </copyright>

namespace Calc;

using System.ComponentModel;
using System.Globalization;

/// <summary>
/// Simple calc that can be used as backend for GUI calc.
/// </summary>
public class Calc : INotifyPropertyChanged
{
    private decimal leftOperand = 0;
    private decimal rightOperand = 0;
    private ArithmeticOperation operation;
    private bool decimalPointPressed = false;
    private decimal decimalMultiplier = 1;
    private State state = State.EnteringFirstOperand;
    private string displayResult = "0";

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    private enum State
    {
        EnteringFirstOperand,
        EnteringNewOperand,
        PressedFirstOperator,
        PressedOperator,
        PressedEquals,
        Error,
    }

    /// <summary>
    /// Gets the result of calculations that can be displayed.
    /// </summary>
    public string DisplayResult
    {
        get => displayResult;
        private set
        {
            displayResult = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayResult)));
        }
    }

    /// <summary>
    /// Enter a digit.
    /// </summary>
    /// <param name="digit">Digit to enter.</param>
    public void EnterDigit(int digit)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(digit, nameof(digit));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(digit, 10, nameof(digit));

        if (state == State.Error)
        {
            return;
        }

        if (state == State.PressedEquals)
        {
            leftOperand = 0;
        }

        state = state switch
        {
            State.EnteringFirstOperand => State.EnteringFirstOperand,
            State.EnteringNewOperand => State.EnteringNewOperand,
            State.PressedFirstOperator => State.EnteringNewOperand,
            State.PressedOperator => State.EnteringNewOperand,
            State.PressedEquals => State.EnteringFirstOperand,
            _ => State.EnteringNewOperand,
        };

        ref var operand = ref state == State.EnteringFirstOperand ? ref leftOperand : ref rightOperand;

        if (!decimalPointPressed)
        {
            operand = (operand * 10) + digit;
        }
        else
        {
            decimalMultiplier /= 10;
            operand += decimalMultiplier * digit;
        }

        DisplayResult = operand.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Add decimal point to the number being entered.
    /// </summary>
    public void EnterDecimalPoint()
    {
        if (state == State.Error)
        {
            return;
        }

        if (decimalPointPressed)
        {
            return;
        }

        if (state is State.EnteringFirstOperand or State.EnteringNewOperand)
        {
            decimalPointPressed = true;
            DisplayResult += '.';
        }
    }

    /// <summary>
    /// Perform an arithmetic operation.
    /// </summary>
    /// <param name="operation">Operation to perform.</param>
    public void OperatorPressed(ArithmeticOperation operation)
    {
        if (state == State.Error)
        {
            return;
        }

        if (decimalPointPressed)
        {
            decimalMultiplier = 1;
            decimalPointPressed = false;
        }

        if (state == State.EnteringNewOperand)
        {
            PerformOperation();
        }

        state = state switch
        {
            State.EnteringFirstOperand => State.PressedFirstOperator,
            State.EnteringNewOperand => State.PressedOperator,
            State.PressedFirstOperator => State.PressedFirstOperator,
            State.PressedOperator => State.PressedOperator,
            State.PressedEquals => State.PressedOperator,
            _ => State.PressedOperator,
        };

        this.operation = operation;
    }

    /// <summary>
    /// Sets <see cref="DisplayResult"/> to the latest calculated value.
    /// </summary>
    public void EqualsPressed()
    {
        if (state == State.Error)
        {
            return;
        }

        if (state is State.EnteringNewOperand or State.PressedOperator)
        {
            PerformOperation();
            if (state != State.Error)
            {
                state = State.PressedEquals;
            }
        }
    }

    /// <summary>
    /// Clears all calculations.
    /// </summary>
    public void Clear()
    {
        leftOperand = 0;
        rightOperand = 0;
        decimalMultiplier = 1;
        decimalPointPressed = false;
        state = State.EnteringFirstOperand;
        DisplayResult = "0";
    }

    private void PerformOperation()
    {
        if (operation == ArithmeticOperation.Division && rightOperand == 0)
        {
            DisplayResult = "Error";
            state = State.Error;
            return;
        }

        leftOperand = operation switch
        {
            ArithmeticOperation.Addition => leftOperand + rightOperand,
            ArithmeticOperation.Subtraction => leftOperand - rightOperand,
            ArithmeticOperation.Multiplication => leftOperand * rightOperand,
            ArithmeticOperation.Division => leftOperand / rightOperand,
            _ => leftOperand,
        };
        rightOperand = 0;
        DisplayResult = leftOperand.ToString(CultureInfo.InvariantCulture);
    }
}
