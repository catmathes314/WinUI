// MainWindow.xaml.cs


// ================================
// haven't multiplication and division before addition and subtraction.
// repeated "=" not works.
// Numerical display needs optimization.
// ================================

using System;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Graphics;
using WinRT.Interop;


namespace WinUICalculator
{
    public sealed partial class MainWindow : Window
    {
        double accumulator = 0;       // left/result 
        string inputBuffer = "";      // right/current input 
        string? pendingOp = null;     // operation
        double? lastOperand = null;   // for repeated "="
        bool justEvaluated = false;

        public MainWindow()
        {
            this.InitializeComponent();

            // Window size
            var hWnd = WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new SizeInt32(450, 600));

            DisplayText.Text = "0";
            WireUpButtons();
        }


        void WireUpButtons()
        {
            foreach (var child in ButtonsGrid.Children)
            {
                if (child is not Button btn) continue;

                var t = btn.Content?.ToString() ?? string.Empty;
                if (string.IsNullOrEmpty(t)) continue;

                if (char.IsDigit(t[0])) btn.Click += NumberButton_Click;
                else if (t == ".") btn.Click += DotButton_Click;
                else if ("+-*/".Contains(t)) btn.Click += OperatorButton_Click;
                else if (t == "=") btn.Click += EqualButton_Click;
                else if (t == "%") btn.Click += PercentButton_Click;
                else if (t == "C") btn.Click += CButton_Click;
                else if (t == "AC") btn.Click += ACButton_Click;
            }
        }

        void UpdateDisplay()
        {
            if (!string.IsNullOrEmpty(inputBuffer))
                DisplayText.Text = inputBuffer;
            else
                DisplayText.Text = Format(accumulator);
        }

        string Format(double v)
        {
            var s = v.ToString("G15", System.Globalization.CultureInfo.InvariantCulture);
            if (s.Contains('.'))
                s = s.TrimEnd('0').TrimEnd('.');
            return s;
        }

        bool TryParseBuffer(out double value)
        {
            if (string.IsNullOrEmpty(inputBuffer))
            {
                value = 0; // avoid "Use of unassigned out parameter 'value'" error
                return false;
            }

            return double.TryParse(
                inputBuffer,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out value);
        }

        bool ApplyPending(double right, out double result)
        {
            try
            {
                result = pendingOp switch
                {
                    "+" => accumulator + right,
                    "-" => accumulator - right,
                    "*" => accumulator * right,
                    "/" => right == 0 ? throw new DivideByZeroException() : accumulator / right,
                    _ => accumulator
                };
                return true;
            }
            catch
            {
                result = 0;
                ShowError();
                return false;
            }
        }

        void ShowError()
        {
            DisplayText.Text = "Error";
            // reset
            accumulator = 0;
            inputBuffer = "";
            pendingOp = null;
            lastOperand = null;
            justEvaluated = false;
        }

        void NumberButton_Click(object sender, RoutedEventArgs e)
        {
            var digit = ((Button)sender).Content.ToString()!;

            if (justEvaluated && pendingOp == null)
            {
                // restart
                accumulator = 0;
                inputBuffer = "";
            }
            justEvaluated = false;

            if (string.IsNullOrEmpty(inputBuffer) || inputBuffer == "0")
            {
                inputBuffer = (digit == "00") ? "0" : digit;
            }
            else inputBuffer += digit;

            UpdateDisplay();
        }

        void DotButton_Click(object sender, RoutedEventArgs e)
        {
            if (justEvaluated && pendingOp == null)
            {
                // restart
                accumulator = 0; inputBuffer = ""; justEvaluated = false;
            }
            if (string.IsNullOrEmpty(inputBuffer)) inputBuffer = "0";
            if (!inputBuffer.Contains(".")) inputBuffer += ".";
            UpdateDisplay();
        }

        void OperatorButton_Click(object sender, RoutedEventArgs e)
        {
            var op = ((Button)sender).Content.ToString()!;

            if (pendingOp != null && TryParseBuffer(out var right))
            {
                if (!ApplyPending(right, out var res)) return;
                accumulator = res;
                lastOperand = right;
            }
            else if (pendingOp == null && TryParseBuffer(out var first))
            {
                accumulator = first;
            }

            pendingOp = op;
            inputBuffer = "";
            justEvaluated = false;
            UpdateDisplay();
        }

        void EqualButton_Click(object sender, RoutedEventArgs e)
        {
            if (pendingOp == null)
            {
                // if haven't right :use left
                if (TryParseBuffer(out var v)) accumulator = v;
                inputBuffer = "";
                justEvaluated = true;
                UpdateDisplay();
                return;
            }

            double right;
            if (TryParseBuffer(out var bufVal))
            {
                right = bufVal;
                lastOperand = right;
            }
            else
            {
                right = lastOperand ?? accumulator;
            }

            if (!ApplyPending(right, out var res)) return;

            accumulator = res;
            pendingOp = null;
            inputBuffer = "";
            justEvaluated = true;
            UpdateDisplay();
        }

        void PercentButton_Click(object sender, RoutedEventArgs e)
        {
            if (pendingOp != null)
            {
                // ex: 8 - % = 7.36
                double baseVal = accumulator;
                double entry;
                if (!TryParseBuffer(out entry))
                {
                    entry = accumulator;
                }
                double newRight = baseVal * entry / 100.0;
                inputBuffer = Format(newRight);
                justEvaluated = false;
                UpdateDisplay();
                return;
            }

            if (TryParseBuffer(out var cur))
                inputBuffer = Format(cur / 100.0);
            else
                accumulator = accumulator / 100.0;

            justEvaluated = false;
            UpdateDisplay();
        }

        // clear current input 
        void CButton_Click(object sender, RoutedEventArgs e)
        {
            inputBuffer = "";
            justEvaluated = false;
            DisplayText.Text = "0";
        }

        // reset
        void ACButton_Click(object sender, RoutedEventArgs e)
        {
            accumulator = 0;
            inputBuffer = "";
            pendingOp = null;
            lastOperand = null;
            justEvaluated = false;
            DisplayText.Text = "0";
        }
    }
}
