using MauiApp16.Models;
using Plugin.LocalNotification;

namespace MauiApp16.Services;

public class NotificationService : MauiApp16.Services.INotificationService
{
    private readonly ISettingsService _settingsService;
    private ITaskService _taskService; // без readonly

    /// <summary>
    /// Plugin.LocalNotification на Windows (WinUI 3) часто кидає COMException «Element not found»
    /// при роботі з планувальником toast — обходимо, щоб не ламати вхід і збереження налаштувань.
    /// </summary>
    private static bool IsLocalNotificationSupported =>
        !OperatingSystem.IsWindows();

    public NotificationService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public void SetTaskService(ITaskService taskService)
    {
        _taskService = taskService;
    }

    public async Task ScheduleNotificationAsync(int id, string title, string message, DateTime scheduledTime)
    {
        if (!IsLocalNotificationSupported)
            return;

        var settings = await _settingsService.GetSettingsAsync();
        if (!settings.NotificationsEnabled)
            return;

        var delay = scheduledTime - DateTime.Now;
        if (delay.TotalSeconds <= 0)
            return;

        var notification = new NotificationRequest
        {
            NotificationId = id,
            Title = title,
            Description = message,
            BadgeNumber = 1,
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = scheduledTime,
                RepeatType = NotificationRepeat.No
            }
        };

        await LocalNotificationCenter.Current.Show(notification);
    }

    public Task CancelNotificationAsync(int id)
    {
        if (IsLocalNotificationSupported)
            LocalNotificationCenter.Current.Cancel(id);
        return Task.CompletedTask;
    }

    public async Task<bool> AreNotificationsEnabledAsync()
    {
        if (!IsLocalNotificationSupported)
            return true;

        return await LocalNotificationCenter.Current.AreNotificationsEnabled();
    }

    public async Task NotifyTodayDeadlinesAsync(List<TaskModel> tasks)
    {
        if (!IsLocalNotificationSupported)
            return;

        var settings = await _settingsService.GetSettingsAsync();
        if (!settings.NotificationsEnabled) return;

        var todayTasks = tasks.Where(t =>
            t.Status != Models.TaskStatus.Completed &&
            t.Deadline.Date == DateTime.Today).ToList();

        if (!todayTasks.Any()) return;

        var titles = string.Join(", ", todayTasks.Take(3).Select(t => t.Title));
        var more = todayTasks.Count > 3 ? $" та ще {todayTasks.Count - 3}" : "";

        var notification = new NotificationRequest
        {
            NotificationId = 88888,
            Title = $"⏰ Сьогодні {todayTasks.Count} дедлайн(ів)!",
            Description = $"{titles}{more}",
            BadgeNumber = todayTasks.Count,
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = DateTime.Now.AddSeconds(3),
                RepeatType = NotificationRepeat.No
            }
        };

        await LocalNotificationCenter.Current.Show(notification);
    }
}
