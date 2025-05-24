using MauiApp2.Models;
using MauiApp2.ViewModels;
using System.Globalization;

namespace MauiApp2.Converters
{
    public class SelectedTermColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var currentTerm = value as Term;
            var page = parameter as MainPage;
            var selectedTerm = page?.BindingContext is MainViewModel vm ? vm.SelectedTerm : null;

            if (currentTerm == selectedTerm)
            {
                return Colors.LightBlue;
            }
            else
            {
                return Colors.White;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

}
