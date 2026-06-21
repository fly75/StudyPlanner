using System.Globalization;

namespace MauiApp16.Converters
{
    public class BoolToNotificationColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // IsRead = true → прочитане → звичайний фон Surface
            // IsRead = false → непрочитане → легкий зелений акцент
            if (value is bool isRead && !isRead)
            {
                if (Application.Current.Resources.TryGetValue("Primary", out var primary))
                    return Color.FromArgb("#1A" + ((Color)primary).ToHex().TrimStart('#'));
            }

            if (Application.Current.Resources.TryGetValue("Surface", out var surface))
                return (Color)surface;

            return Colors.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
