using System.Globalization;

namespace MauiApp16.Converters;

public class DateTimeToTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime dateTime)
            return dateTime.TimeOfDay;
        return TimeSpan.Zero;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TimeSpan time)
        {
            // беремо поточну дату з Binding
            if (targetType == typeof(DateTime))
            {
                var today = DateTime.Today;
                return today.Add(time);
            }
        }

        return DateTime.Now;
    }

}