using MauiApp16.Models;

namespace MauiApp16.Services;

public interface INotificationService
{
    Task ScheduleNotificationAsync(int id, string title, string message, DateTime scheduledTime);
    Task CancelNotificationAsync(int id);
    Task<bool> AreNotificationsEnabledAsync();
    Task NotifyTodayDeadlinesAsync(List<TaskModel> tasks);
}