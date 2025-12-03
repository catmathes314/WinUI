// CalculatorViewModel.cs

// ========================================================================
// haven't multiplication and division before addition and subtraction.
// Perhaps the arithmetic logic needs to be modified. 9+= and 9*%= do not work.
// Numerical display needs optimization.
// ========================================================================

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace WinUICalculator
{
    public class CalculatorViewModel : INotifyPropertyChanged
    {

        double accumulator = 0;        // left/result 
        string inputBuffer = "";       // right/current input 
        string? pendingOp = null;      // operation
        double? lastOperand = null;    // for repeated "="
        bool justEvaluated = false;

        // display (for binding)
        string _display = "0";
        public string Display
        {
            get => _display;
            private set
            {
                if (_display == value) return;
                _display = value;
                OnPropertyChanged();
            }
        }

        public ICommand ButtonCommand { get; }

        public CalculatorViewModel()
        {
            ButtonCommand = new RelayCommand(OnButtonPressed);
        }

        void OnButtonPressed(object? parameter)
        {
            var key = parameter?.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(key)) return;

            if (char.IsDigit(key[0]) || key == "00")
            {
                HandleDigit(key);
            }
            else if (key == ".")
            {
                HandleDot();
            }
            else if ("+-*/".Contains(key))
            {
                HandleOperator(key);
            }
            else if (key == "=")
            {
                HandleEqual();
            }
            else if (key == "%")
            {
                HandlePercent();
            }
            else if (key == "C")
            {
                HandleClear();
            }
            else if (key == "AC")
            {
                HandleAllClear();
            }
        }


        void HandleDigit(string digit)
        {
            if (justEvaluated)
            {
                accumulator = 0;
                inputBuffer = "";
                pendingOp = null;
                lastOperand = null;
            }
            justEvaluated = false;

            if (string.IsNullOrEmpty(inputBuffer) || inputBuffer == "0")
            {
                inputBuffer = (digit == "00") ? "0" : digit;
            }
            else
            {
                inputBuffer += digit;
            }

            UpdateDisplay();
        }


        void HandleDot()
        {
            if (justEvaluated && pendingOp == null)
            {
                accumulator = 0;
                inputBuffer = "";
                justEvaluated = false;
            }

            if (string.IsNullOrEmpty(inputBuffer))
                inputBuffer = "0.";

            else if (!inputBuffer.Contains("."))
                inputBuffer += ".";

            UpdateDisplay();
        }

        void HandleOperator(string op)
        {
            if (!string.IsNullOrEmpty(inputBuffer))
            {
                var evalOp = pendingOp;

                if (!TryEvaluate(inputBuffer, evalOp, out var result))
                    return;

                accumulator = result;
                inputBuffer = "";
            }

            pendingOp = op;
            justEvaluated = false;
            UpdateDisplay();
        }


        void HandleEqual()
        {
            string operandText = inputBuffer;

            if (string.IsNullOrEmpty(operandText))
            {
                if (lastOperand is null || string.IsNullOrEmpty(pendingOp))
                {
                    // nothing to do
                    return;
                }
                operandText = lastOperand.Value.ToString();
            }

            if (!TryEvaluate(operandText, pendingOp, out var result))
                return;

            accumulator = result;
            lastOperand = double.Parse(operandText);
            inputBuffer = "";
            justEvaluated = true;
            UpdateDisplay();
        }

        void HandlePercent()
        {
            if (string.IsNullOrEmpty(inputBuffer))
                return;

            if (!double.TryParse(inputBuffer, out var v))
                return;

            v = v / 100.0;

            inputBuffer = v.ToString();
            justEvaluated = false;

            UpdateDisplay();
        }



        void HandleClear()
        {
            inputBuffer = "";
            justEvaluated = false;
            UpdateDisplay();
        }

        void HandleAllClear()
        {
            accumulator = 0;
            inputBuffer = "";
            pendingOp = null;
            lastOperand = null;
            justEvaluated = false;
            Display = "0";
        }

        void UpdateDisplay()
        {
            if (!string.IsNullOrEmpty(inputBuffer))
                Display = inputBuffer;
            else
                Display = Format(accumulator);
        }

        string Format(double v)
        {
            var s = v.ToString("G15", System.Globalization.CultureInfo.InvariantCulture);
            if (s.Contains('.'))
                s = s.TrimEnd('0').TrimEnd('.');
            return s;
        }

        bool TryEvaluate(string rightText, string? op, out double result)
        {
            try
            {
                var right = double.Parse(rightText);

                if (op is null)
                {
                    result = right;
                    return true;
                }

                result = op switch
                {
                    "+" => accumulator + right,
                    "-" => accumulator - right,
                    "*" => accumulator * right,
                    "/" when right != 0 => accumulator / right,
                    "/" when right == 0 => throw new DivideByZeroException(),
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
            Display = "Error";
            // reset
            accumulator = 0;
            inputBuffer = "";
            pendingOp = null;
            lastOperand = null;
            justEvaluated = false;
        }

        // ========== INotifyPropertyChanged ==========

        public event PropertyChangedEventHandler? PropertyChanged;

        void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
