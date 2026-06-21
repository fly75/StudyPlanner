using SQLite;

namespace MauiApp16.Models;

public class Course : IEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Color { get; set; }
    public DateTime CreatedAt { get; set; }
    public double Progress { get; set; }
}
