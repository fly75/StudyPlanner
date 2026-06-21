using MauiApp16.Data;
using MauiApp16.Models;

namespace MauiApp16.Services;

public class TestDataService : ITestDataService
{
    private readonly CourseRepository _courseRepository;
    private readonly TaskRepository _taskRepository;
    private readonly StatisticsRepository _statisticsRepository;
    private readonly IAuthService _authService;

    public TestDataService(
        CourseRepository courseRepository,
        TaskRepository taskRepository,
        StatisticsRepository statisticsRepository,
        IAuthService authService)
    {
        _courseRepository = courseRepository;
        _taskRepository = taskRepository;
        _statisticsRepository = statisticsRepository;
        _authService = authService;
    }

    public async Task<bool> HasTestDataAsync()
    {
        var user = await _authService.GetCurrentUserAsync();
        if (user == null) return false;

        var courses = await _courseRepository.GetByUserIdAsync(user.Id);
        return courses.Any();
    }

    public async Task SeedTestDataAsync()
    {
        var user = await _authService.GetCurrentUserAsync();
        if (user == null) return;

        // Перевірка чи вже є дані
        var existingCourses = await _courseRepository.GetByUserIdAsync(user.Id);
        if (existingCourses.Any()) return;

        // Створюємо тестові курси
        var courses = new[]
        {
            new Course
            {
                UserId = user.Id,
                Name = "Математичний аналіз",
                Description = "Диференціальне та інтегральне числення",
                Color = "#F05454",
                CreatedAt = DateTime.Now.AddDays(-30),
                Progress = 65
            },
            new Course
            {
                UserId = user.Id,
                Name = "Програмування C#",
                Description = "Основи ООП та .NET розробки",
                Color = "#3AAFA9",
                CreatedAt = DateTime.Now.AddDays(-25),
                Progress = 80
            },
            new Course
            {
                UserId = user.Id,
                Name = "Бази даних",
                Description = "SQL, нормалізація, транзакції",
                Color = "#5B8CDB",
                CreatedAt = DateTime.Now.AddDays(-20),
                Progress = 45
            },
            new Course
            {
                UserId = user.Id,
                Name = "Англійська мова",
                Description = "Технічна англійська для IT",
                Color = "#F4845F",
                CreatedAt = DateTime.Now.AddDays(-15),
                Progress = 55
            },
            new Course
            {
                UserId = user.Id,
                Name = "Дискретна математика",
                Description = "Теорія графів та комбінаторика",
                Color = "#52B788",
                CreatedAt = DateTime.Now.AddDays(-10),
                Progress = 30
            }
        };

        var courseIds = new List<int>();
        foreach (var course in courses)
        {
            await _courseRepository.SaveAsync(course);
            courseIds.Add(course.Id);
        }

        // Створюємо тестові завдання
        var tasks = new[]
        {
            // Математичний аналіз
            new TaskModel
            {
                CourseId = courseIds[0],
                Title = "Домашня робота №5",
                Description = "Розв'язати задачі 15-25 з підручника",
                Deadline = DateTime.Now.AddDays(2),
                Priority = TaskPriority.High,
                Status = Models.TaskStatus.InProgress,
                CreatedAt = DateTime.Now.AddDays(-2),
                TagsRaw = "домашня,практична"
            },
            new TaskModel
            {
                CourseId = courseIds[0],
                Title = "Підготовка до контрольної",
                Description = "Повторити теми: похідні та інтеграли",
                Deadline = DateTime.Now.AddDays(5),
                Priority = TaskPriority.High,
                Status = Models.TaskStatus.Pending,
                CreatedAt = DateTime.Now.AddDays(-1),
                TagsRaw = "іспит"
            },
            new TaskModel
            {
                CourseId = courseIds[0],
                Title = "Конспект лекції",
                Description = "Законспектувати матеріал з теми 'Ряди'",
                Deadline = DateTime.Now.AddDays(-3),
                Priority = TaskPriority.Low,
                Status = Models.TaskStatus.Completed,
                CreatedAt = DateTime.Now.AddDays(-5),
                CompletedAt = DateTime.Now.AddDays(-3)
                // без тегів
            },

            // Програмування C#
            new TaskModel
            {
                CourseId = courseIds[1],
                Title = "Лабораторна робота №3",
                Description = "Реалізувати калькулятор з GUI",
                Deadline = DateTime.Now.AddDays(7),
                Priority = TaskPriority.High,
                Status = Models.TaskStatus.InProgress,
                CreatedAt = DateTime.Now.AddDays(-3),
                TagsRaw = "лабораторна"
            },
            new TaskModel
            {
                CourseId = courseIds[1],
                Title = "Вивчити LINQ",
                Description = "Переглянути туторіали по LINQ запитам",
                Deadline = DateTime.Now.AddDays(4),
                Priority = TaskPriority.Medium,
                Status = Models.TaskStatus.Pending,
                CreatedAt = DateTime.Now.AddDays(-1)
                // без тегів
            },
            new TaskModel
            {
                CourseId = courseIds[1],
                Title = "Практична робота №2",
                Description = "Консольний додаток для роботи з файлами",
                Deadline = DateTime.Now.AddDays(-2),
                Priority = TaskPriority.High,
                Status = Models.TaskStatus.Completed,
                CreatedAt = DateTime.Now.AddDays(-7),
                CompletedAt = DateTime.Now.AddDays(-2),
                TagsRaw = "практична,лабораторна"
            },

            // Бази даних
            new TaskModel
            {
                CourseId = courseIds[2],
                Title = "Курсовий проект",
                Description = "Розробити схему БД для магазину",
                Deadline = DateTime.Now.AddDays(14),
                Priority = TaskPriority.High,
                Status = Models.TaskStatus.Pending,
                CreatedAt = DateTime.Now,
                TagsRaw = "курсова,проект"
            },
            new TaskModel
            {
                CourseId = courseIds[2],
                Title = "SQL запити",
                Description = "Виконати 10 складних JOIN запитів",
                Deadline = DateTime.Now.AddDays(3),
                Priority = TaskPriority.Medium,
                Status = Models.TaskStatus.InProgress,
                CreatedAt = DateTime.Now.AddDays(-2),
                TagsRaw = "практична"
            },
            new TaskModel
            {
                CourseId = courseIds[2],
                Title = "Тест по нормалізації",
                Description = "Підготуватися до тесту",
                Deadline = DateTime.Now.AddDays(-1),
                Priority = TaskPriority.Medium,
                Status = Models.TaskStatus.Completed,
                CreatedAt = DateTime.Now.AddDays(-4),
                CompletedAt = DateTime.Now.AddDays(-1),
                TagsRaw = "іспит"
            },

            // Англійська мова
            new TaskModel
            {
                CourseId = courseIds[3],
                Title = "Есе про технології",
                Description = "Написати есе на 500 слів",
                Deadline = DateTime.Now.AddDays(6),
                Priority = TaskPriority.Medium,
                Status = Models.TaskStatus.Pending,
                CreatedAt = DateTime.Now,
                TagsRaw = "реферат"
            },
            new TaskModel
            {
                CourseId = courseIds[3],
                Title = "Вивчити слова Unit 5",
                Description = "50 нових технічних термінів",
                Deadline = DateTime.Now.AddDays(1),
                Priority = TaskPriority.Low,
                Status = Models.TaskStatus.InProgress,
                CreatedAt = DateTime.Now.AddDays(-2),
                TagsRaw = "домашня"
            },

            // Дискретна математика
            new TaskModel
            {
                CourseId = courseIds[4],
                Title = "Задачі з теорії графів",
                Description = "Розв'язати задачі 1-15",
                Deadline = DateTime.Now.AddDays(8),
                Priority = TaskPriority.Medium,
                Status = Models.TaskStatus.Pending,
                CreatedAt = DateTime.Now,
                TagsRaw = "домашня,практична"
            },
            new TaskModel
            {
                CourseId = courseIds[4],
                Title = "Реферат",
                Description = "Алгоритм Дейкстри",
                Deadline = DateTime.Now.AddDays(12),
                Priority = TaskPriority.Low,
                Status = Models.TaskStatus.Pending,
                CreatedAt = DateTime.Now,
                TagsRaw = "реферат,проект"
            }
        };

        foreach (var task in tasks)
        {
            await _taskRepository.SaveAsync(task);
        }

        // Створюємо статистику за останні 14 днів
        for (int i = 14; i > 0; i--)
        {
            var date = DateTime.Now.AddDays(-i).Date;
            var completedTasks = new Random().Next(0, 5);

            var stat = new Statistics
            {
                UserId = user.Id,
                Date = date,
                CompletedTasks = completedTasks,
                TotalTasks = 13,
                StudyMinutes = new Random().Next(30, 180)
            };

            await _statisticsRepository.SaveAsync(stat);
        }
    }
}