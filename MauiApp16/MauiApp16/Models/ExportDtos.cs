using System.Text.Json.Serialization;

namespace MauiApp16.Models
{
    public class ExportDtos
    {
        /// <summary>Верхній рівень файлу, згенерованого ExportService.ExportToJsonAsync</summary>
        public class ExportedTaskFile
        {
            [JsonPropertyName("exportedAt")]
            public string ExportedAt { get; set; } = string.Empty;

            [JsonPropertyName("totalTasks")]
            public int TotalTasks { get; set; }

            [JsonPropertyName("completedTasks")]
            public int CompletedTasks { get; set; }

            [JsonPropertyName("tasks")]
            public List<ExportedTaskItem> Tasks { get; set; } = new();
        }

        public class ExportedTaskItem
        {
            [JsonPropertyName("title")]
            public string Title { get; set; } = string.Empty;

            [JsonPropertyName("description")]
            public string Description { get; set; } = string.Empty;

            [JsonPropertyName("course")]
            public string Course { get; set; } = string.Empty;

            [JsonPropertyName("deadline")]
            public string Deadline { get; set; } = string.Empty;

            [JsonPropertyName("priority")]
            public string Priority { get; set; } = string.Empty;

            [JsonPropertyName("status")]
            public string Status { get; set; } = string.Empty;

            [JsonPropertyName("createdAt")]
            public string CreatedAt { get; set; } = string.Empty;

            [JsonPropertyName("completedAt")]
            public string CompletedAt { get; set; } = string.Empty;
        }
    }
}
