using System.Globalization;

namespace MauiApp16.Converters
{
    // Активний тег = Primary, неактивний = Surface
    public class BoolToTagColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSelected && isSelected)
                return Application.Current.Resources["Primary"];
            return Application.Current.Resources["Surface"];
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
