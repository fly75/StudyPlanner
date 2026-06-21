using SQLite;

namespace MauiApp16.Models;

public class Statistics : IEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime Date { get; set; }
    public int CompletedTasks { get; set; }
    public int TotalTasks { get; set; }
    public int StudyMinutes { get; set; }
}