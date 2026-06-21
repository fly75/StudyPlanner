using MauiApp16.Models;

namespace MauiApp16.Data
{
    public class AppNotificationRepository : IRepository<AppNotification>
    {
        private readonly DatabaseContext _context;
        public AppNotificationRepository(DatabaseContext context) => _context = context;

        public Task<List<AppNotification>> GetAllAsync() =>
            _context.GetAllAsync<AppNotification>();

        public async Task<List<AppNotification>> GetByUserIdAsync(int userId) =>
            await _context.QueryAsync<AppNotification>(
                "SELECT * FROM AppNotification WHERE UserId = ? ORDER BY CreatedAt DESC", userId);

        public Task<AppNotification> GetByIdAsync(int id) =>
            _context.GetAsync<AppNotification>(id);

        public Task<int> SaveAsync(AppNotification item) =>
            _context.SaveAsync(item);

        public Task<int> DeleteAsync(AppNotification item) =>
            _context.DeleteAsync(item);

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            var list = await _context.QueryAsync<AppNotification>(
                "SELECT * FROM AppNotification WHERE UserId = ? AND IsRead = 0", userId);
            return list.Count;
        }

        /// <summary>Перевірити, чи вже є сповіщення для цього завдання сьогодні (dedup).</summary>
        public async Task<bool> ExistsForTaskTodayAsync(int userId, int taskId)
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var list = await _context.QueryAsync<AppNotification>(
                "SELECT * FROM AppNotification WHERE UserId=? AND TaskId=? AND CreatedAt>=? AND CreatedAt<?",
                userId, taskId, today, tomorrow);
            return list.Any();
        }
    }
}
