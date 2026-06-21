using MauiApp16.Data;
using MauiApp16.Models;

namespace MauiApp16.Services
{
    public class InAppNotificationService : IInAppNotificationService
    {
        private readonly AppNotificationRepository _repository;
        private readonly IAuthService _authService;

        public InAppNotificationService(
            AppNotificationRepository repository,
            IAuthService authService)
        {
            _repository = repository;
            _authService = authService;
        }

        public async Task AddNotificationAsync(string title, string message, int? taskId = null)
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null) return;

            // Уникаємо дублювання: якщо сповіщення для цього завдання вже є сьогодні — пропускаємо
            if (taskId.HasValue && await _repository.ExistsForTaskTodayAsync(user.Id, taskId.Value))
                return;

            await _repository.SaveAsync(new AppNotification
            {
                UserId = user.Id,
                Title = title,
                Message = message,
                CreatedAt = DateTime.Now,
                IsRead = false,
                TaskId = taskId
            });
        }

        public async Task<List<AppNotification>> GetNotificationsAsync(bool unreadOnly = false)
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null) return new List<AppNotification>();

            var all = await _repository.GetByUserIdAsync(user.Id);
            return unreadOnly ? all.Where(n => !n.IsRead).ToList() : all;
        }

        public async Task<int> GetUnreadCountAsync()
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null) return 0;
            return await _repository.GetUnreadCountAsync(user.Id);
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var n = await _repository.GetByIdAsync(notificationId);
            if (n != null && !n.IsRead)
            {
                n.IsRead = true;
                await _repository.SaveAsync(n);
            }
        }
    }
}
