using System.Globalization;
using MauiApp16.Models;

namespace MauiApp16.Converters;

public class TaskStatusToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Models.TaskStatus status)
            return status == Models.TaskStatus.Completed;
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isCompleted)
            return isCompleted ? Models.TaskStatus.Completed : Models.TaskStatus.InProgress;
        return Models.TaskStatus.Pending;
    }
}