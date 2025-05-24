using System.Globalization;

namespace MauiApp2.Converters
{
    public class GreaterThanZeroConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is int intValue && intValue > 0;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => (bool)value ? 1 : 0;
    }
}
