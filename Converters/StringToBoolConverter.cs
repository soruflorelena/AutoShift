using System.Globalization;

namespace AutoShift.Converters
{
    public class StringToBoolConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (parameter != null)
                return value?.ToString() == parameter?.ToString();

            return !string.IsNullOrWhiteSpace(value?.ToString());
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b && b)
            {
                return parameter?.ToString();
            }

            return null;
        }
    }
}
