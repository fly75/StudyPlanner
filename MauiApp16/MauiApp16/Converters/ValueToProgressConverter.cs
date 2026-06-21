using System.Globalization;

namespace MauiApp16.Converters;

public class ValueToProgressConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return 0.0;

        double taskCount = 0;

        if (value is int intValue)
            taskCount = intValue;
        else if (value is double doubleValue)
            taskCount = doubleValue;
        else
            return 0.0;

        if (taskCount == 0)
            return 0.0;

        // Максимальна ширина для відображення (вся ширина батьківського елемента)
        // Припускаємо максимум 10 завдань на день для шкали
        var maxTasks = 10.0;

        // Обмежуємо максимальну ширину
        var percentage = Math.Min(taskCount / maxTasks, 1.0);

        // Повертаємо відносну ширину (батьківський Grid автоматично масштабує)
        // Мінімальна ширина 50 пікселів для видимості
        var baseWidth = 300.0; // Базова ширина
        var calculatedWidth = percentage * baseWidth;

        return Math.Max(calculatedWidth, taskCount > 0 ? 50 : 0);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}