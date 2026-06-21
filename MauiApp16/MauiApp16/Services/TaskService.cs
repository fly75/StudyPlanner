using MauiApp16.Data;
using MauiApp16.Models;

namespace MauiApp16.Services;

public class TaskService : ITaskService
{
    private readonly TaskRepository _taskRepository;
    private readonly CourseRepository _courseRepository;
    private readonly INotificationService _notificationService;
    private readonly IAuthService _authService;
    private readonly ICourseService _courseService;
    private readonly ISettingsService _settingsService;
    private readonly IInAppNotificationService _inAppNotificationService;

    public TaskService(
        TaskRepository taskRepository,
        CourseRepository courseRepository,
        INotificationService notificationService,
        IAuthService authService,
        ICourseService courseService,
        ISettingsService settingsService,
        IInAppNotificationService inAppNotificationService)
    {
        _taskRepository = taskRepository;
        _courseRepository = courseRepository;
        _notificationService = notificationService;
        _authService = authService;
        _courseService = courseService;
        _settingsService = settingsService;
        _inAppNotificationService = inAppNotificationService;
    }

    public async Task<List<TaskModel>> GetAllTasksAsync()
    {
        var tasks = await _taskRepository.GetAllAsync();
        return await EnrichTasksWithCourseInfo(tasks);
    }

    public async Task<List<TaskModel>> GetTasksByCourseAsync(int courseId)
    {
        var tasks = await _taskRepository.GetByCourseIdAsync(courseId);
        return await EnrichTasksWithCourseInfo(tasks);
    }

    public async Task<List<TaskModel>> GetUpcomingTasksAsync(int days)
    {
        var user = await _authService.GetCurrentUserAsync();
        if (user == null)
            return new List<TaskModel>();

        var courses = await _courseRepository.GetByUserIdAsync(user.Id);
        var allTasks = new List<TaskModel>();

        foreach (var course in courses)
        {
            var from = DateTime.Now;
            var to = DateTime.Now.AddDays(days);
            var tasks = await _taskRepository.GetUpcomingAsync(from, to);
            var courseTasks = tasks.Where(t => t.CourseId == course.Id).ToList();
            allTasks.AddRange(courseTasks);
        }

        return await EnrichTasksWithCourseInfo(allTasks);
    }

    public async Task<TaskModel> GetTaskByIdAsync(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task != null)
        {
            var tasks = await EnrichTasksWithCourseInfo(new List<TaskModel> { task });
            return tasks.FirstOrDefault();
        }
        return null;
    }

    public async Task<bool> SaveTaskAsync(TaskModel task)
    {
        if (task.Id == 0)
        {
            task.CreatedAt = DateTime.Now;
            task.Status = Models.TaskStatus.Pending;
        }

        await _taskRepository.SaveAsync(task);

        if (task.Status != Models.TaskStatus.Completed)
        {
            var settings = await _settingsService.GetSettingsAsync();
            var notifyTime = task.Deadline.AddMinutes(-settings.ReminderMinutesBefore);

            // Формуємо читабельний текст часу
            var reminderText = settings.ReminderMinutesBefore switch
            {
                10080 => "7 днів",
                4320 => "3 дні",
                1440 => "1 день",
                720 => "12 годин",
                180 => "3 години",
                60 => "1 годину",
                _ => $"{settings.ReminderMinutesBefore} хвилин"
            };

            var pushTitle = $"⏰ Дедлайн через {reminderText}!";
            var pushMessage = $"📌 {task.Title}\n🗓 {task.Deadline:dd.MM.yyyy HH:mm}";

            // Push-сповіщення
            await _notificationService.ScheduleNotificationAsync(task.Id, pushTitle, pushMessage, notifyTime);

            // In-app сповіщення — одразу
            await _inAppNotificationService.AddNotificationAsync(pushTitle, pushMessage, task.Id);
        }

        await _courseService.UpdateCourseProgressAsync(task.CourseId);
        return true;
    }

    public async Task<bool> DeleteTaskAsync(int taskId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task != null)
        {
            var courseId = task.CourseId;

            await _notificationService.CancelNotificationAsync(taskId);
            await _taskRepository.DeleteAsync(task);

            // Оновлюємо прогрес курсу
            await _courseService.UpdateCourseProgressAsync(courseId);

            return true;
        }
        return false;
    }

    public async Task<bool> CompleteTaskAsync(int taskId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task != null)
        {
            task.Status = Models.TaskStatus.Completed;
            task.CompletedAt = DateTime.Now;
            await _taskRepository.SaveAsync(task);
            await _notificationService.CancelNotificationAsync(taskId);

            // Оновлюємо прогрес курсу
            await _courseService.UpdateCourseProgressAsync(task.CourseId);

            return true;
        }
        return false;
    }

    private async Task<List<TaskModel>> EnrichTasksWithCourseInfo(List<TaskModel> tasks)
    {
        foreach (var task in tasks)
        {
            var course = await _courseRepository.GetByIdAsync(task.CourseId);
            if (course != null)
            {
                task.CourseName = course.Name;
                task.CourseColor = course.Color;
            }
        }
        return tasks;
    }

    public async Task<List<TaskModel>> SearchTasksAsync(
        int courseId,
        string query,
        Models.TaskStatus? status,
        DateTime? deadlineFrom,
        DateTime? deadlineTo)
    {
        // Беремо всі завдання курсу (або всі, якщо courseId == 0)
        List<TaskModel> tasks;
        if (courseId > 0)
            tasks = await _taskRepository.GetByCourseIdAsync(courseId);
        else
            tasks = await _taskRepository.GetAllAsync();

        // Фільтруємо в пам'яті — дані вже завантажені, запити до БД не потрібні
        if (!string.IsNullOrWhiteSpace(query))
            tasks = tasks.Where(t =>
                t.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                (t.Description?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)
            ).ToList();

        if (status.HasValue)
            tasks = tasks.Where(t => t.Status == status.Value).ToList();

        if (deadlineFrom.HasValue)
            tasks = tasks.Where(t => t.Deadline.Date >= deadlineFrom.Value.Date).ToList();

        if (deadlineTo.HasValue)
            tasks = tasks.Where(t => t.Deadline.Date <= deadlineTo.Value.Date).ToList();

        return await EnrichTasksWithCourseInfo(tasks);
    }
}