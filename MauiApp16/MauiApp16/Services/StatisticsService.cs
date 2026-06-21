using MauiApp16.Data;
using MauiApp16.Models;

namespace MauiApp16.Services;

public class StatisticsService : IStatisticsService
{
    private readonly StatisticsRepository _statisticsRepository;
    private readonly TaskRepository _taskRepository;
    private readonly CourseRepository _courseRepository;
    private readonly IAuthService _authService;

    public StatisticsService(
        StatisticsRepository statisticsRepository,
        TaskRepository taskRepository,
        CourseRepository courseRepository,
        IAuthService authService)
    {
        _statisticsRepository = statisticsRepository;
        _taskRepository = taskRepository;
        _courseRepository = courseRepository;
        _authService = authService;
    }

    // ── Хелпер: усі завдання поточного користувача ──────────
    private async Task<List<TaskModel>> GetCurrentUserTasksAsync()
    {
        var user = await _authService.GetCurrentUserAsync();
        if (user == null) return new List<TaskModel>();

        var courses = await _courseRepository.GetByUserIdAsync(user.Id);
        var allTasks = new List<TaskModel>();

        foreach (var course in courses)
        {
            var tasks = await _taskRepository.GetByCourseIdAsync(course.Id);
            allTasks.AddRange(tasks);
        }

        return allTasks;
    }

    public async Task<Dictionary<string, int>> GetCompletedTasksChartDataAsync(int days)
    {
        var user = await _authService.GetCurrentUserAsync();
        if (user == null) return new Dictionary<string, int>();

        var allTasks = await GetCurrentUserTasksAsync();
        var chartData = new Dictionary<string, int>();
        var today = DateTime.Now.Date;

        for (int i = days - 1; i >= 0; i--)
        {
            var date = today.AddDays(-i);
            var count = allTasks.Count(t =>
                t.Status == Models.TaskStatus.Completed &&
                t.CompletedAt.HasValue &&
                t.CompletedAt.Value.Date == date);

            chartData[date.ToString("dd.MM")] = count;
        }

        return chartData;
    }

    public async Task<Dictionary<string, double>> GetTimeByCoursesAsync()
    {
        var courses = await _courseRepository.GetAllAsync();
        var result = new Dictionary<string, double>();

        foreach (var course in courses)
        {
            var tasks = await _taskRepository.GetByCourseIdAsync(course.Id);
            var completedCount = tasks.Count(t => t.Status == Models.TaskStatus.Completed);
            result[course.Name] = completedCount;
        }

        return result;
    }

    public async Task<double> GetCompletionPercentageAsync()
    {
        var allTasks = await GetCurrentUserTasksAsync();
        if (allTasks.Count == 0) return 0;

        var completed = allTasks.Count(t => t.Status == Models.TaskStatus.Completed);
        return Math.Round((double)completed / allTasks.Count * 100, 2);
    }

    public async Task UpdateDailyStatisticsAsync()
    {
        var user = await _authService.GetCurrentUserAsync();
        if (user == null) return;

        var today = DateTime.Now.Date;
        var allTasks = await _taskRepository.GetAllAsync();
        var completedToday = allTasks.Count(t => t.CompletedAt?.Date == today);

        var stat = new Statistics
        {
            UserId = user.Id,
            Date = today,
            CompletedTasks = completedToday,
            TotalTasks = allTasks.Count,
            StudyMinutes = 0
        };

        await _statisticsRepository.SaveAsync(stat);
    }

    public async Task<double> GetAverageCompletionTimeHoursAsync()
    {
        var allTasks = await GetCurrentUserTasksAsync();

        var completed = allTasks
            .Where(t => t.Status == Models.TaskStatus.Completed && t.CompletedAt.HasValue)
            .ToList();

        if (!completed.Any()) return 0;

        var totalHours = completed.Sum(t =>
            (t.CompletedAt!.Value - t.CreatedAt).TotalHours);

        return Math.Round(totalHours / completed.Count, 1);
    }

    public async Task<Dictionary<DayOfWeek, int>> GetProductivityByWeekDayAsync()
    {
        var allTasks = await GetCurrentUserTasksAsync();

        // Ініціалізуємо всі дні нулями
        var result = Enum.GetValues<DayOfWeek>()
                         .ToDictionary(d => d, _ => 0);

        foreach (var t in allTasks.Where(t =>
            t.Status == Models.TaskStatus.Completed && t.CompletedAt.HasValue))
        {
            result[t.CompletedAt!.Value.DayOfWeek]++;
        }

        return result;
    }

    public async Task<int> GetCurrentStreakAsync()
    {
        var allTasks = await GetCurrentUserTasksAsync();

        var dates = allTasks
            .Where(t => t.Status == Models.TaskStatus.Completed && t.CompletedAt.HasValue)
            .Select(t => t.CompletedAt!.Value.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        if (!dates.Any()) return 0;

        var today = DateTime.Today;
        var yesterday = today.AddDays(-1);

        // Серія повинна включати сьогодні або вчора
        if (!dates.Contains(today) && !dates.Contains(yesterday)) return 0;

        var streak = 0;
        var checkDate = dates.Contains(today) ? today : yesterday;

        while (dates.Contains(checkDate))
        {
            streak++;
            checkDate = checkDate.AddDays(-1);
        }

        return streak;
    }

    public async Task<int> GetBestStreakAsync()
    {
        var allTasks = await GetCurrentUserTasksAsync();

        var dates = allTasks
            .Where(t => t.Status == Models.TaskStatus.Completed && t.CompletedAt.HasValue)
            .Select(t => t.CompletedAt!.Value.Date)
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        if (!dates.Any()) return 0;

        int best = 1;
        int current = 1;

        for (int i = 1; i < dates.Count; i++)
        {
            if ((dates[i] - dates[i - 1]).Days == 1)
            {
                current++;
                if (current > best) best = current;
            }
            else
            {
                current = 1;
            }
        }

        return best;
    }

    public async Task<Dictionary<TaskPriority, int>> GetTasksByPriorityAsync()
    {
        var allTasks = await GetCurrentUserTasksAsync();

        return new Dictionary<TaskPriority, int>
        {
            [TaskPriority.High] = allTasks.Count(t => t.Priority == TaskPriority.High),
            [TaskPriority.Medium] = allTasks.Count(t => t.Priority == TaskPriority.Medium),
            [TaskPriority.Low] = allTasks.Count(t => t.Priority == TaskPriority.Low),
        };
    }

    public async Task<(string CourseName, int CompletedCount)> GetTopCourseAsync()
    {
        var user = await _authService.GetCurrentUserAsync();
        if (user == null) return ("—", 0);

        var courses = await _courseRepository.GetByUserIdAsync(user.Id);
        var best = ("—", 0);

        foreach (var course in courses)
        {
            var tasks = await _taskRepository.GetByCourseIdAsync(course.Id);
            var completed = tasks.Count(t => t.Status == Models.TaskStatus.Completed);

            if (completed > best.Item2)
                best = (course.Name, completed);
        }

        return best;
    }
}