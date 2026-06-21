using SQLite;

namespace MauiApp16.Models
{
    public class AppNotification : IEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int UserId { get; set; }

        /// <summary>Заголовок (копія push-нотифікації)</summary>
        public string Title { get; set; }

        /// <summary>Текст (копія push-нотифікації)</summary>
        public string Message { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsRead { get; set; }

        /// <summary>ID завдання, якщо сповіщення пов'язане із завданням</summary>
        public int? TaskId { get; set; }
    }
}
