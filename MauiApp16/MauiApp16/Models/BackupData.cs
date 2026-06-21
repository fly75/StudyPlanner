
namespace MauiApp16.Models
{
    /// <summary>
    /// Кореневий об'єкт JSON-бекапу.
    /// Зберігає всі дані користувача БЕЗ пароля.
    /// </summary>
    public class BackupData
    {
        public string Version { get; set; } = "1.0";
        public string AppName { get; set; } = "Study Planner";
        public string ExportedAt { get; set; }
        public string UserEmail { get; set; }

        public List<CourseBackup> Courses { get; set; } = new();
        public List<TaskBackup> Tasks { get; set; } = new();
        public List<StatisticsBackup> Statistics { get; set; } = new();
    }

    /// <summary>Предмет без UserId (прив'язка до поточного юзера при імпорті).</summary>
    public class CourseBackup
    {
        public int OriginalId { get; set; }   // Старий ID — потрібен для маппінгу Tasks
        public string Name { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
        public DateTime CreatedAt { get; set; }
        public double Progress { get; set; }
    }

    /// <summary>Завдання — CourseId буде перемапований при імпорті.</summary>
    public class TaskBackup
    {
        public int OriginalCourseId { get; set; }  // Старий CourseId
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Deadline { get; set; }
        public TaskPriority Priority { get; set; }
        public TaskStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string TagsRaw { get; set; }
    }

    /// <summary>Статистика — UserId прив'яжеться до поточного юзера.</summary>
    public class StatisticsBackup
    {
        public DateTime Date { get; set; }
        public int CompletedTasks { get; set; }
        public int TotalTasks { get; set; }
        public int StudyMinutes { get; set; }
    }
}
