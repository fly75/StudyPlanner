using System.Globalization;

namespace MauiApp16.Converters;

public class ProgressWidthConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || values.Length < 2)
            return 0.0;

        // values[0] - Progress (0-100)
        // values[1] - Container Width

        double progress = 0;
        double containerWidth = 0;

        if (values[0] is double progressValue)
            progress = progressValue;
        else if (values[0] is int progressInt)
            progress = progressInt;

        if (values[1] is double widthValue)
            containerWidth = widthValue;

        // Якщо ширина контейнера ще не визначена
        if (containerWidth <= 0)
            return 0.0;

        // Конвертуємо відсоток (0-100) в ширину
        var calculatedWidth = (progress / 100.0) * containerWidth;

        // Мінімальна ширина для видимості
        return Math.Max(0, calculatedWidth);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}