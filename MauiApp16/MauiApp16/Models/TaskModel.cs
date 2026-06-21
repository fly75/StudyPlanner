using SQLite;

namespace MauiApp16.Models;

public class TaskModel : IEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime Deadline { get; set; }
    public TaskPriority Priority { get; set; }
    public TaskStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public string TagsRaw { get; set; } = string.Empty;

    // Не зберігається в БД — обчислюється з TagsRaw
    [Ignore]
    public List<string> Tags
    {
        get => string.IsNullOrWhiteSpace(TagsRaw)
            ? new List<string>()
            : TagsRaw.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
        set => TagsRaw = value != null ? string.Join(",", value) : string.Empty;
    }

    [Ignore]
    public string CourseName { get; set; }
    [Ignore]
    public string CourseColor { get; set; }
}

public enum TaskPriority
{
    Low = 0,
    Medium = 1,
    High = 2
}

public enum TaskStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2
}