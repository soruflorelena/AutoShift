namespace AutoShift.Converters
{
    public class ExpandedToTextConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool isExpanded)
            {
                return isExpanded ? "OCULTAR" : "VER DETALLES";
            }
            return "VER";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            return value?.ToString() == "OCULTAR";
        }
    }
}
