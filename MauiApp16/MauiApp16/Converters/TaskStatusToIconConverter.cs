using System.Globalization;
using MauiApp16.Models;

namespace MauiApp16.Converters;

public class TaskStatusToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Models.TaskStatus status)
        {
            return status switch
            {
                Models.TaskStatus.Pending => "⏱",
                Models.TaskStatus.InProgress => "🔄",
                Models.TaskStatus.Completed => "✅",
                _ => "⏱"
            };
        }
        return "⏱";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}