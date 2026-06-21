using System.Globalization;
using MauiApp16.Models;

namespace MauiApp16 .Converters;

public class PriorityToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TaskPriority priority)
        {
            return priority switch
            {
                TaskPriority.Low => "Низький пріоритет",
                TaskPriority.Medium => "Середній пріоритет",
                TaskPriority.High => "Високий пріоритет",
                _ => ""
            };
        }
        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}