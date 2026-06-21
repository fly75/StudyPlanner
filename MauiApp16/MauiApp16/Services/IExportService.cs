using MauiApp16.Models;

namespace MauiApp16.Services
{
    public interface IExportService
    {
        Task<string> ExportToCsvAsync(List<TaskModel> tasks, string fileName);
        Task<string> ExportToJsonAsync(List<TaskModel> tasks, string fileName);
        Task<string> ExportToTxtAsync(List<TaskModel> tasks, string fileName, string courseName = "Всі предмети");
        Task ShareFileAsync(string filePath, string title);
    }
}
