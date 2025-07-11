using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace FactorySIM.Converters
{
    /// <summary>
    /// Converts boolean IsRunning property to display text.
    /// </summary>
    public class BoolToRunningConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isRunning)
            {
                return isRunning ? "Running" : "Stopped";
            }
            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("BoolToRunningConverter is one-way only");
        }
    }

    /// <summary>
    /// Inverts a boolean value (true becomes false, false becomes true).
    /// Useful for enabling/disabling buttons based on simulation state.
    /// </summary>
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return true; // Default to enabled if conversion fails
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return false;
        }
    }

    /// <summary>
    /// Converts boolean IsLowStock to appropriate color (red for low stock, black for normal).
    /// </summary>
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isLowStock)
            {
                return isLowStock ? Brushes.Red : Brushes.Black;
            }
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("BoolToColorConverter is one-way only");
        }
    }

    /// <summary>
    /// Converts boolean to Visibility (true = Visible, false = Collapsed).
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }
            return false;
        }
    }

    /// <summary>
    /// Converts boolean IsBusy to background color (busy = light yellow, idle = white).
    /// </summary>
    public class BusyToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isBusy)
            {
                return isBusy ? new SolidColorBrush(Color.FromRgb(255, 255, 200)) : Brushes.White;
            }
            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("BusyToBackgroundConverter is one-way only");
        }
    }

    /// <summary>
    /// Converts decimal cost values to formatted currency strings.
    /// </summary>
    public class CurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal decimalValue)
            {
                return $"${decimalValue:F2}";
            }
            if (value is double doubleValue)
            {
                return $"${doubleValue:F2}";
            }
            if (value is float floatValue)
            {
                return $"${floatValue:F2}";
            }
            return "$0.00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue && stringValue.StartsWith("$"))
            {
                var numberPart = stringValue.Substring(1);
                if (decimal.TryParse(numberPart, out decimal result))
                {
                    return result;
                }
            }
            return 0m;
        }
    }

    /// <summary>
    /// Converts TimeSpan to human-readable format (e.g., "45 minutes", "1.5 hours").
    /// </summary>
    public class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan timeSpan)
            {
                if (timeSpan.TotalMinutes < 60)
                {
                    return $"{timeSpan.TotalMinutes:0} min";
                }
                else if (timeSpan.TotalHours < 24)
                {
                    return $"{timeSpan.TotalHours:F1} hrs";
                }
                else
                {
                    return $"{timeSpan.TotalDays:F1} days";
                }
            }
            return "0 min";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("TimeSpanToStringConverter is one-way only");
        }
    }

    /// <summary>
    /// Converts operation priority to appropriate color (higher priority = more urgent color).
    /// </summary>
    public class PriorityToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int priority)
            {
                return priority switch
                {
                    1 => Brushes.Red,      // High priority
                    2 => Brushes.Orange,   // Medium priority
                    3 => Brushes.Green,    // Normal priority
                    _ => Brushes.Gray      // Low/unknown priority
                };
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("PriorityToColorConverter is one-way only");
        }
    }

    /// <summary>
    /// Multi-converter that determines if an operation can be executed (all conditions met).
    /// </summary>
    public class OperationAvailabilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // This would need access to the Factory to check availability
            // For now, return a default value
            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("OperationAvailabilityConverter is one-way only");
        }
    }
}