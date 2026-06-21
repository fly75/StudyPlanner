using System.Text;
using System.Text.Json;
using MauiApp16.Models;

using TaskStatus = MauiApp16.Models.TaskStatus;
using TaskPriority = MauiApp16.Models.TaskPriority;

namespace MauiApp16.Services
{
    public class ExportService : IExportService
    {
        // ─── CSV ───────────────────────────────────────────────────────────────
        public async Task<string> ExportToCsvAsync(List<TaskModel> tasks, string fileName)
        {
            var sb = new StringBuilder();

            // Заголовок — екрануємо лапками на випадок ком у тексті
            sb.AppendLine("\"Назва\",\"Опис\",\"Предмет\",\"Дедлайн\",\"Пріоритет\",\"Статус\",\"Створено\",\"Завершено\"");

            foreach (var t in tasks)
            {
                sb.AppendLine(string.Join(",", new[]
                {
                CsvEscape(t.Title),
                CsvEscape(t.Description ?? ""),
                CsvEscape(t.CourseName ?? ""),
                CsvEscape(t.Deadline.ToString("dd.MM.yyyy HH:mm")),
                CsvEscape(PriorityToString(t.Priority)),
                CsvEscape(StatusToString(t.Status)),
                CsvEscape(t.CreatedAt.ToString("dd.MM.yyyy HH:mm")),
                CsvEscape(t.CompletedAt?.ToString("dd.MM.yyyy HH:mm") ?? "")
            }));
            }

            return await SaveFileAsync(fileName + ".csv", sb.ToString());
        }

        // ─── JSON ──────────────────────────────────────────────────────────────
        public async Task<string> ExportToJsonAsync(List<TaskModel> tasks, string fileName)
        {
            // Створюємо анонімний об'єкт — чисто для читабельного JSON без SQLite-атрибутів
            var export = new
            {
                ExportedAt = DateTime.Now.ToString("dd.MM.yyyy HH:mm"),
                TotalTasks = tasks.Count,
                CompletedTasks = tasks.Count(t => t.Status == TaskStatus.Completed),
                Tasks = tasks.Select(t => new
                {
                    t.Title,
                    Description = t.Description ?? "",
                    Course = t.CourseName ?? "",
                    Deadline = t.Deadline.ToString("dd.MM.yyyy HH:mm"),
                    Priority = PriorityToString(t.Priority),
                    Status = StatusToString(t.Status),
                    CreatedAt = t.CreatedAt.ToString("dd.MM.yyyy HH:mm"),
                    CompletedAt = t.CompletedAt?.ToString("dd.MM.yyyy HH:mm") ?? ""
                }).ToList()
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(export, options);

            return await SaveFileAsync(fileName + ".json", json);
        }

        // ─── TXT (читабельний звіт) ────────────────────────────────────────────
        public async Task<string> ExportToTxtAsync(List<TaskModel> tasks, string fileName, string courseName = "Всі предмети")
        {
            var sb = new StringBuilder();
            var line = new string('─', 50);

            sb.AppendLine("╔══════════════════════════════════════════════════╗");
            sb.AppendLine("║           STUDY PLANNER — ЗВІТ ПО ЗАВДАННЯХ      ║");
            sb.AppendLine("╚══════════════════════════════════════════════════╝");
            sb.AppendLine();
            sb.AppendLine($"Предмет:        {courseName}");
            sb.AppendLine($"Дата звіту:     {DateTime.Now:dd.MM.yyyy HH:mm}");
            sb.AppendLine($"Всього завдань: {tasks.Count}");
            sb.AppendLine($"Завершено:      {tasks.Count(t => t.Status == TaskStatus.Completed)}");
            sb.AppendLine($"В процесі:      {tasks.Count(t => t.Status == TaskStatus.InProgress)}");
            sb.AppendLine($"Очікують:       {tasks.Count(t => t.Status == TaskStatus.Pending)}");
            sb.AppendLine();
            sb.AppendLine(line);

            // Групуємо по статусу для зручного читання
            var grouped = tasks
                .GroupBy(t => t.Status)
                .OrderBy(g => g.Key);

            foreach (var group in grouped)
            {
                sb.AppendLine();
                sb.AppendLine($"▶ {StatusToString(group.Key).ToUpper()} ({group.Count()})");
                sb.AppendLine(line);

                foreach (var t in group.OrderBy(t => t.Deadline))
                {
                    sb.AppendLine($"  📌 {t.Title}");
                    if (!string.IsNullOrWhiteSpace(t.Description))
                        sb.AppendLine($"     {t.Description}");
                    sb.AppendLine($"     Предмет:   {t.CourseName}");
                    sb.AppendLine($"     Дедлайн:   {t.Deadline:dd.MM.yyyy HH:mm}");
                    sb.AppendLine($"     Пріоритет: {PriorityToString(t.Priority)}");
                    if (t.CompletedAt.HasValue)
                        sb.AppendLine($"     Завершено: {t.CompletedAt:dd.MM.yyyy HH:mm}");
                    sb.AppendLine();
                }
            }

            sb.AppendLine(line);
            sb.AppendLine($"Згенеровано Study Planner • {DateTime.Now:dd.MM.yyyy}");

            return await SaveFileAsync(fileName + ".txt", sb.ToString());
        }

        // ─── Share ─────────────────────────────────────────────────────────────
        public async Task ShareFileAsync(string filePath, string title)
        {
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = title,
                File = new ShareFile(filePath)
            });
        }

        // ─── Helpers ───────────────────────────────────────────────────────────
        private async Task<string> SaveFileAsync(string fileName, string content)
        {
            // CacheDirectory — тимчасово, ідеально для Share
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
            await File.WriteAllTextAsync(filePath, content, Encoding.UTF8);
            return filePath;
        }

        private static string CsvEscape(string value)
        {
            // Якщо є лапки, коми або переноси — обгортаємо у лапки і екрануємо внутрішні
            if (value.Contains('"') || value.Contains(',') || value.Contains('\n'))
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return $"\"{value}\"";
        }

        private static string PriorityToString(TaskPriority priority) => priority switch
        {
            TaskPriority.High => "Високий",
            TaskPriority.Medium => "Середній",
            TaskPriority.Low => "Низький",
            _ => priority.ToString()
        };

        private static string StatusToString(TaskStatus status) => status switch
        {
            TaskStatus.Pending => "Очікує",
            TaskStatus.InProgress => "В процесі",
            TaskStatus.Completed => "Завершено",
            _ => status.ToString()
        };
    }
}
