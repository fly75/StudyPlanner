using System.Globalization;

namespace MauiApp16.Converters
{
    // Фон заголовка дня: сьогодні = Primary, інші = Surface
    public class BoolToTodayColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isToday && isToday)
                return Application.Current.Resources["Primary"];
            return Application.Current.Resources["Surface"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}