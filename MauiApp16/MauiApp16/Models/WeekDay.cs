namespace MauiApp16.Models;

public class WeekDay
{
    public DateTime Date { get; set; }
    public string DayName { get; set; }      // "Пн", "Вт" ...
    public string DayNumber { get; set; }    // "12", "13" ...
    public List<TaskModel> Tasks { get; set; } = new();
    public bool IsToday => Date.Date == DateTime.Today;
    public bool HasTasks => Tasks.Count > 0;
}