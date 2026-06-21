using MauiApp16.Models;
 
namespace MauiApp16.Services;

public interface IStatisticsService
{
    Task<Dictionary<string, int>> GetCompletedTasksChartDataAsync(int days);
    Task<Dictionary<string, double>> GetTimeByCoursesAsync();
    Task<double> GetCompletionPercentageAsync();
    Task UpdateDailyStatisticsAsync();

    /// <summary>Середній час від створення до виконання завдання (у годинах).</summary>
    Task<double> GetAverageCompletionTimeHoursAsync();

    /// <summary>К-сть виконаних завдань для кожного дня тижня (пн..нд).</summary>
    Task<Dictionary<DayOfWeek, int>> GetProductivityByWeekDayAsync();

    /// <summary>Поточна серія активності (consecutive days з ≥1 виконаним завданням).</summary>
    Task<int> GetCurrentStreakAsync();

    /// <summary>Найдовша серія активності за весь час.</summary>
    Task<int> GetBestStreakAsync();

    /// <summary>Розбивка завдань по пріоритетах.</summary>
    Task<Dictionary<TaskPriority, int>> GetTasksByPriorityAsync();

    /// <summary>Предмет з найбільшою кількістю виконаних завдань.</summary>
    Task<(string CourseName, int CompletedCount)> GetTopCourseAsync();
}