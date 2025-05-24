using System.Globalization;

namespace MauiApp2.Converters
{
    //https://github.com/xceedsoftware/wpftoolkit/blob/master/ExtendedWPFToolkitSolution/Src/Xceed.Wpf.Toolkit/Core/Converters/NullToBoolConverter.cs
    public class NullToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value != null;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
