using System.Text;
using System.Text.Json;
using MauiApp16.Data;
using MauiApp16.Models;

namespace MauiApp16.Services
{
    public class BackupService : IBackupService
    {
        private readonly IAuthService _authService;
        private readonly CourseRepository _courseRepository;
        private readonly TaskRepository _taskRepository;
        private readonly StatisticsRepository _statisticsRepository;

        private const string BackupFileName = "studyplanner_backup.json";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public BackupService(
            IAuthService authService,
            CourseRepository courseRepository,
            TaskRepository taskRepository,
            StatisticsRepository statisticsRepository)
        {
            _authService = authService;
            _courseRepository = courseRepository;
            _taskRepository = taskRepository;
            _statisticsRepository = statisticsRepository;
        }

        // Експорт
        public async Task<string> ExportBackupAsync()
        {
            var user = await _authService.GetCurrentUserAsync()
                ?? throw new InvalidOperationException("Користувач не авторизований.");

            // Збираємо дані
            var courses = await _courseRepository.GetByUserIdAsync(user.Id);
            var allStats = await _statisticsRepository.GetAllAsync();
            var userStats = allStats.Where(s => s.UserId == user.Id).ToList();

            var allTasks = new List<TaskModel>();
            foreach (var course in courses)
            {
                var tasks = await _taskRepository.GetByCourseIdAsync(course.Id);
                allTasks.AddRange(tasks);
            }

            // Будуємо об'єкт бекапу
            var backup = new BackupData
            {
                ExportedAt = DateTime.Now.ToString("dd.MM.yyyy HH:mm"),
                UserEmail = user.Email,
                Courses = courses.Select(c => new CourseBackup
                {
                    OriginalId = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Color = c.Color,
                    CreatedAt = c.CreatedAt,
                    Progress = c.Progress,
                }).ToList(),

                Tasks = allTasks.Select(t => new TaskBackup
                {
                    OriginalCourseId = t.CourseId,
                    Title = t.Title,
                    Description = t.Description,
                    Deadline = t.Deadline,
                    Priority = t.Priority,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt,
                    CompletedAt = t.CompletedAt,
                    TagsRaw = t.TagsRaw,
                }).ToList(),

                Statistics = userStats.Select(s => new StatisticsBackup
                {
                    Date = s.Date,
                    CompletedTasks = s.CompletedTasks,
                    TotalTasks = s.TotalTasks,
                    StudyMinutes = s.StudyMinutes,
                }).ToList(),
            };

            // Серіалізуємо та зберігаємо
            var json = JsonSerializer.Serialize(backup, JsonOptions);
            var filePath = Path.Combine(FileSystem.CacheDirectory, BackupFileName);
            await File.WriteAllTextAsync(filePath, json, Encoding.UTF8);

            // Відкриваємо Share-діалог
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = $"Study Planner — бекап {DateTime.Now:dd.MM.yyyy}",
                File = new ShareFile(filePath),
            });

            return filePath;
        }

        // Імпорт
        public async Task<(bool Success, string Message)> ImportBackupAsync()
        {
            // Вибір файлу
            var pickResult = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Оберіть файл бекапу (.json)",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI,   new[] { ".json" } },
                    { DevicePlatform.Android, new[] { "application/json", "*/*" } },
                    { DevicePlatform.iOS,     new[] { "public.json", "public.text" } },
                    { DevicePlatform.MacCatalyst, new[] { "json" } },
                }),
            });

            if (pickResult == null)
                return (false, "Файл не обрано.");

            // Читаємо та парсимо
            string json;
            try
            {
                using var stream = await pickResult.OpenReadAsync();
                using var reader = new StreamReader(stream, Encoding.UTF8);
                json = await reader.ReadToEndAsync();
            }
            catch (Exception ex)
            {
                return (false, $"Помилка читання файлу: {ex.Message}");
            }

            BackupData backup;
            try
            {
                backup = JsonSerializer.Deserialize<BackupData>(json, JsonOptions)
                    ?? throw new FormatException("Порожній або пошкоджений файл.");
            }
            catch (Exception ex)
            {
                return (false, $"Невірний формат бекапу: {ex.Message}");
            }

            // Перевірка версії
            if (backup.AppName != "Study Planner")
                return (false, "Це не бекап Study Planner.");

            // Поточний користувач
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
                return (false, "Користувач не авторизований.");

            try
            {
                // Видаляємо поточні дані
                var existingCourses = await _courseRepository.GetByUserIdAsync(user.Id);
                foreach (var c in existingCourses)
                {
                    await _taskRepository.DeleteByCourseIdAsync(c.Id);
                    await _courseRepository.DeleteAsync(c);
                }

                var allStats = await _statisticsRepository.GetAllAsync();
                var userStats = allStats.Where(s => s.UserId == user.Id).ToList();
                foreach (var s in userStats)
                    await _statisticsRepository.DeleteAsync(s);

                // Відновлюємо предмети + будуємо маппінг ID
                // oldCourseId → newCourseId
                var idMap = new Dictionary<int, int>();

                foreach (var cb in backup.Courses)
                {
                    var course = new Course
                    {
                        UserId = user.Id,
                        Name = cb.Name,
                        Description = cb.Description,
                        Color = cb.Color,
                        CreatedAt = cb.CreatedAt,
                        Progress = cb.Progress,
                    };

                    await _courseRepository.SaveAsync(course);
                    idMap[cb.OriginalId] = course.Id;
                }

                // Відновлюємо завдання
                foreach (var tb in backup.Tasks)
                {
                    // Якщо CourseId не знайдено в маппінгу — пропускаємо
                    if (!idMap.TryGetValue(tb.OriginalCourseId, out var newCourseId))
                        continue;

                    var task = new TaskModel
                    {
                        CourseId = newCourseId,
                        Title = tb.Title,
                        Description = tb.Description,
                        Deadline = tb.Deadline,
                        Priority = tb.Priority,
                        Status = tb.Status,
                        CreatedAt = tb.CreatedAt,
                        CompletedAt = tb.CompletedAt,
                        TagsRaw = tb.TagsRaw ?? string.Empty,
                    };

                    await _taskRepository.SaveAsync(task);
                }

                // Відновлюємо статистику
                foreach (var sb in backup.Statistics)
                {
                    var stat = new Statistics
                    {
                        UserId = user.Id,
                        Date = sb.Date,
                        CompletedTasks = sb.CompletedTasks,
                        TotalTasks = sb.TotalTasks,
                        StudyMinutes = sb.StudyMinutes,
                    };

                    await _statisticsRepository.SaveAsync(stat);
                }

                var summary = $"Відновлено: {backup.Courses.Count} предметів, " +
                              $"{backup.Tasks.Count} завдань, " +
                              $"{backup.Statistics.Count} записів статистики.\n" +
                              $"Бекап від: {backup.ExportedAt}";

                return (true, summary);
            }
            catch (Exception ex)
            {
                return (false, $"Помилка відновлення: {ex.Message}");
            }
        }

        // Інфо про останній бекап

        public Task<(DateTime? Date, long SizeBytes)> GetLastBackupInfoAsync()
        {
            var filePath = Path.Combine(FileSystem.CacheDirectory, BackupFileName);

            if (!File.Exists(filePath))
                return Task.FromResult<(DateTime?, long)>((null, 0));

            var info = new FileInfo(filePath);
            return Task.FromResult<(DateTime?, long)>((info.LastWriteTime, info.Length));
        }

        // Merge-імпорт (без видалення існуючих)
        public async Task<(bool Success, string Message)> MergeImportBackupAsync()
        {
            // Вибір файлу
            var pickResult = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Оберіть файл бекапу (.json)",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI,       new[] { ".json" } },
                    { DevicePlatform.Android,     new[] { "application/json", "*/*" } },
                    { DevicePlatform.iOS,         new[] { "public.json", "public.text" } },
                    { DevicePlatform.MacCatalyst, new[] { "json" } },
                }),
            });

            if (pickResult == null)
                return (false, "Файл не обрано.");

            // Читаємо
            string json;
            try
            {
                using var stream = await pickResult.OpenReadAsync();
                using var reader = new StreamReader(stream, Encoding.UTF8);
                json = await reader.ReadToEndAsync();
            }
            catch (Exception ex)
            {
                return (false, $"Помилка читання файлу: {ex.Message}");
            }

            BackupData backup;
            try
            {
                backup = JsonSerializer.Deserialize<BackupData>(json, JsonOptions)
                    ?? throw new FormatException("Порожній або пошкоджений файл.");
            }
            catch (Exception ex)
            {
                return (false, $"Невірний формат бекапу: {ex.Message}");
            }

            if (backup.AppName != "Study Planner")
                return (false, "Це не бекап Study Planner.");

            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
                return (false, "Користувач не авторизований.");

            try
            {
                // Завантажуємо вже існуючі предмети (щоб не дублювати)
                var existingCourses = await _courseRepository.GetByUserIdAsync(user.Id);

                // oldId (з бекапу) → реальний courseId у БД
                var idMap = new Dictionary<int, int>();

                int addedCourses = 0;
                int reusedCourses = 0;

                foreach (var cb in backup.Courses)
                {
                    // Шукаємо існуючий предмет за назвою (без урахування регістру)
                    var existing = existingCourses
                        .FirstOrDefault(c => string.Equals(
                            c.Name.Trim(), cb.Name.Trim(),
                            StringComparison.OrdinalIgnoreCase));

                    if (existing != null)
                    {
                        // Предмет вже є — просто зберігаємо маппінг
                        idMap[cb.OriginalId] = existing.Id;
                        reusedCourses++;
                    }
                    else
                    {
                        // Нового предмету не існує — створюємо
                        var course = new Course
                        {
                            UserId = user.Id,
                            Name = cb.Name,
                            Description = cb.Description,
                            Color = cb.Color,
                            CreatedAt = cb.CreatedAt,
                            Progress = 0,
                        };

                        await _courseRepository.SaveAsync(course);
                        idMap[cb.OriginalId] = course.Id;
                        existingCourses.Add(course); // оновлюємо локальний список
                        addedCourses++;
                    }
                }

                // Додаємо завдання
                int addedTasks = 0;

                foreach (var tb in backup.Tasks)
                {
                    if (!idMap.TryGetValue(tb.OriginalCourseId, out var targetCourseId))
                        continue;

                    var task = new TaskModel
                    {
                        CourseId = targetCourseId,
                        Title = tb.Title,
                        Description = tb.Description,
                        Deadline = tb.Deadline,
                        Priority = tb.Priority,
                        Status = tb.Status,
                        CreatedAt = tb.CreatedAt,
                        CompletedAt = tb.CompletedAt,
                        TagsRaw = tb.TagsRaw ?? string.Empty,
                    };

                    await _taskRepository.SaveAsync(task);
                    addedTasks++;
                }

                var msg = $"Додано: {addedCourses} нових предметів " +
                          $"({reusedCourses} існуючих), {addedTasks} завдань.\n" +
                          $"Бекап від: {backup.ExportedAt}";

                return (true, msg);
            }
            catch (Exception ex)
            {
                return (false, $"Помилка злиття: {ex.Message}");
            }
        }

        // Імпорт одного завдання з ExportService JSON
        public async Task<(bool Success, string Message)> ImportSingleTaskAsync()
        {
            // Вибір файлу
            var pickResult = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Оберіть файл завдання (.json)",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI,       new[] { ".json" } },
                    { DevicePlatform.Android,     new[] { "application/json", "*/*" } },
                    { DevicePlatform.iOS,         new[] { "public.json", "public.text" } },
                    { DevicePlatform.MacCatalyst, new[] { "json" } },
                }),
            });

            if (pickResult == null)
                return (false, "Файл не обрано.");

            // Читаємо
            string json;
            try
            {
                using var stream = await pickResult.OpenReadAsync();
                using var reader = new StreamReader(stream, Encoding.UTF8);
                json = await reader.ReadToEndAsync();
            }
            catch (Exception ex)
            {
                return (false, $"Помилка читання файлу: {ex.Message}");
            }

            // Парсимо формат ExportService
            // Очікуємо: { "ExportedAt":..., "Tasks": [ { "Title", "Description",
            //   "Course", "Deadline" (dd.MM.yyyy HH:mm), "Priority", "Status", ... } ] }
            ExportDtos.ExportedTaskFile exportedFile;
            try
            {
                var opts = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                exportedFile = JsonSerializer.Deserialize<ExportDtos.ExportedTaskFile>(json, opts)
                    ?? throw new FormatException("Порожній файл.");
            }
            catch (Exception ex)
            {
                return (false, $"Невірний формат файлу: {ex.Message}");
            }

            if (exportedFile.Tasks == null || exportedFile.Tasks.Count == 0)
                return (false, "У файлі не знайдено жодного завдання.");

            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
                return (false, "Користувач не авторизований.");

            try
            {
                // Кешуємо предмети 
                var existingCourses = await _courseRepository.GetByUserIdAsync(user.Id);

                int addedCount = 0;

                foreach (var et in exportedFile.Tasks)
                {
                    // Знаходимо або створюємо предмет
                    var courseName = string.IsNullOrWhiteSpace(et.Course)
                        ? "Без предмету"
                        : et.Course.Trim();

                    var course = existingCourses.FirstOrDefault(c =>
                        string.Equals(c.Name.Trim(), courseName, StringComparison.OrdinalIgnoreCase));

                    if (course == null)
                    {
                        course = new Course
                        {
                            UserId = user.Id,
                            Name = courseName,
                            Description = $"Створено автоматично при імпорті завдання",
                            Color = "#4CAF50",
                            CreatedAt = DateTime.Now,
                            Progress = 0,
                        };
                        await _courseRepository.SaveAsync(course);
                        existingCourses.Add(course);
                    }

                    // Парсимо поля
                    DateTime deadline = DateTime.Now.AddDays(7);
                    if (!string.IsNullOrWhiteSpace(et.Deadline))
                        DateTime.TryParseExact(
                            et.Deadline,
                            new[] { "dd.MM.yyyy HH:mm", "dd.MM.yyyy", "yyyy-MM-ddTHH:mm:ss", "o" },
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None,
                            out deadline);

                    var priority = et.Priority?.ToLowerInvariant() switch
                    {
                        "високий" or "high" => TaskPriority.High,
                        "низький" or "low" => TaskPriority.Low,
                        _ => TaskPriority.Medium,
                    };

                    var status = et.Status?.ToLowerInvariant() switch
                    {
                        "завершено" or "completed" => Models.TaskStatus.Completed,
                        "в процесі" or "inprogress" => Models.TaskStatus.InProgress,
                        _ => Models.TaskStatus.Pending,
                    };

                    var task = new TaskModel
                    {
                        CourseId = course.Id,
                        Title = et.Title ?? "Без назви",
                        Description = et.Description ?? string.Empty,
                        Deadline = deadline,
                        Priority = priority,
                        Status = status,
                        CreatedAt = DateTime.Now,
                        TagsRaw = string.Empty,
                    };

                    await _taskRepository.SaveAsync(task);
                    addedCount++;
                }

                return (true, $"Імпортовано {addedCount} завдань успішно.");
            }
            catch (Exception ex)
            {
                return (false, $"Помилка імпорту: {ex.Message}");
            }
        }
    }
}