using System.Globalization;
using MauiApp16.Models;

namespace MauiApp16.Converters;

public class TaskNotCompletedConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Models.TaskStatus status)
            return status != Models.TaskStatus.Completed;
        return true;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}