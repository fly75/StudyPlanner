using MauiApp16.Models;

namespace MauiApp16.Services
{
    public interface IInAppNotificationService
    {
        Task AddNotificationAsync(string title, string message, int? taskId = null);
        Task<List<AppNotification>> GetNotificationsAsync(bool unreadOnly = false);
        Task<int> GetUnreadCountAsync();
        Task MarkAsReadAsync(int notificationId);
    }
}
