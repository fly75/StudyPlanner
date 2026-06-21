using System.Globalization;

namespace MauiApp16.Converters
{
    // Колір тексту заголовка дня: сьогодні = Gray900 (темний), інші = TextPrimary
    public class BoolToTodayTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isToday && isToday)
                return Application.Current.Resources["Gray900"];
            return Application.Current.Resources["TextPrimary"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
