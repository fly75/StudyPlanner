
namespace MauiApp16.Services
{
    public interface IBackupService
    {
        /// <summary>
        /// Збирає всі дані поточного користувача, серіалізує у JSON
        /// і відкриває системний Share-діалог для збереження/відправки файлу.
        /// Повертає шлях до тимчасового файлу.
        /// </summary>
        Task<string> ExportBackupAsync();

        /// <summary>
        /// Відкриває FilePicker, читає JSON-бекап, видаляє поточні дані
        /// і відновлює з бекапу. Повертає (success, message).
        /// </summary>
        Task<(bool Success, string Message)> ImportBackupAsync();

        /// <summary>Дата і розмір останнього бекапу (з CacheDirectory).</summary>
        Task<(DateTime? Date, long SizeBytes)> GetLastBackupInfoAsync();

        /// <summary>
        /// Зливає бекап з поточними даними БЕЗ видалення існуючих.
        /// Предмети з однаковою назвою не дублюються — задання додаються до існуючого предмету.
        /// Повертає (success, message).
        /// </summary>
        Task<(bool Success, string Message)> MergeImportBackupAsync();

        /// <summary>
        /// Імпортує одне або кілька завдань з файлу ExportService-формату (.json).
        /// Якщо предмет з такою назвою не існує — створює його.
        /// Повертає (success, message).
        /// </summary>
        Task<(bool Success, string Message)> ImportSingleTaskAsync();
    }
}
