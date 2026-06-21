using System.Globalization;

namespace MauiApp16.Converters
{
    public class BoolToFilterColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (bool)value
                ? Color.FromArgb("#4ECDC4")   // активний фільтр — акцентний колір
                : Application.Current.Resources["Surface"];

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
