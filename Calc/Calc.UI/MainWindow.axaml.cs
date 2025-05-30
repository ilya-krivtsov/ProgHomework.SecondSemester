// <copyright file="MainWindow.axaml.cs" company="Ilya Krivtsov">
// Copyright (c) Ilya Krivtsov. All rights reserved.
// </copyright>

namespace Calc.UI;

using System;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;

public partial class MainWindow : Window
{
    private readonly Calc calc;

    public MainWindow()
    {
        InitializeComponent();

        calc = new();

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int digit = ((2 - i) * 3) + j + 1;

                var button = new Button
                {
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                    HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    FontSize = 48,
                    Margin = new(4),
                    Content = digit,
                };

                button.Click += (_, _) => calc.EnterDigit(digit);

                Grid.SetRow(button, i + 2);
                Grid.SetColumn(button, j);

                buttonsGrid.Children.Add(button);
            }
        }

        var binding = new Binding
        {
            Source = calc,
            Path = nameof(calc.DisplayResult),
        };

        resultText.Bind(ContentProperty, binding);
    }

    private void OnClear(object sender, RoutedEventArgs e) => calc.Clear();

    private void OnEquals(object sender, RoutedEventArgs e) => calc.EqualsPressed();

    private void OnZero(object sender, RoutedEventArgs e) => calc.EnterDigit(0);

    private void OnDecimal(object sender, RoutedEventArgs e) => calc.EnterDecimalPoint();

    private void OnOperator(object sender, RoutedEventArgs e)
    {
        if (sender is Button b && b.Tag is string tag && Enum.TryParse<ArithmeticOperation>(tag, out var op))
        {
            calc.OperatorPressed(op);
        }
    }
}
