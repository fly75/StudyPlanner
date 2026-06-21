using System.Globalization;

namespace MauiApp16.Converters
{
    // Текст активного тегу = SurfaceLight (світлий), неактивного = TextSecondary
    public class BoolToTagTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSelected && isSelected)
                return Application.Current.Resources["SurfaceLight"];
            return Application.Current.Resources["TextSecondary"];
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}