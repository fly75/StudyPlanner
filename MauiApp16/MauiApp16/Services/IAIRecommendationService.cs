using MauiApp16.Models;

namespace MauiApp16.Services
{
    /// <summary>Модель запиту для генерації навчального плану через AI.</summary>
    public class AIRecommendationRequest
    {
        public string SubjectName { get; set; } = string.Empty;
        public string SubjectDescription { get; set; } = string.Empty;
        public int TaskCount { get; set; } = 10;
        public string DifficultyLevel { get; set; } = "Середній";
        public List<string> SelectedTaskTypes { get; set; } = new();
        public string StudyTime { get; set; } = "В різний час";
        public string AdditionalPreferences { get; set; } = string.Empty;
    }

    public interface IAIRecommendationService
    {
        /// <summary>
        /// Генерує BackupData через Anthropic API на основі параметрів опитування.
        /// </summary>
        Task<BackupData> GenerateStudyPlanAsync(AIRecommendationRequest request, string apiKey);
    }
}