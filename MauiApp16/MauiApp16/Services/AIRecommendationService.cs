using MauiApp16.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MauiApp16.Services;

public class AIRecommendationService : IAIRecommendationService
{
    private const string ApiUrl = "https://api.anthropic.com/v1/messages";
    private const string Model = "claude-sonnet-4-20250514";

    public async Task<BackupData> GenerateStudyPlanAsync(
        AIRecommendationRequest request,
        string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API-ключ не вказано.");

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("x-api-key", apiKey.Trim());
        client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        client.Timeout = TimeSpan.FromSeconds(120);

        var today = DateTime.Today;
        var taskTypes = request.SelectedTaskTypes.Any()
            ? string.Join(", ", request.SelectedTaskTypes)
            : "різноманітні завдання";

        var prompt = BuildPrompt(request, today, taskTypes);

        var requestBody = JsonSerializer.Serialize(new
        {
            model = Model,
            max_tokens = 4096,
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        });

        HttpResponseMessage response;
        try
        {
            var httpContent = new StringContent(requestBody, Encoding.UTF8, "application/json");
            response = await client.PostAsync(ApiUrl, httpContent);
        }
        catch (TaskCanceledException)
        {
            throw new Exception("Час очікування вичерпано. Перевірте з'єднання з інтернетом.");
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Помилка мережі: {ex.Message}");
        }

        if (!response.IsSuccessStatusCode)
        {
            var errBody = await response.Content.ReadAsStringAsync();

            // Намагаємось витягнути повідомлення з відповіді API
            string apiMessage = string.Empty;
            try
            {
                using var errDoc = JsonDocument.Parse(errBody);
                if (errDoc.RootElement.TryGetProperty("error", out var errEl)
                    && errEl.TryGetProperty("message", out var msgEl))
                    apiMessage = msgEl.GetString() ?? string.Empty;
            }
            catch { /* ігноруємо */ }

            var code = (int)response.StatusCode;
            var userMessage = code switch
            {
                401 => "Невірний API-ключ. Перевірте ключ на console.anthropic.com",
                403 => "Доступ заборонено. Можливо ключ відкликано або немає квоти.",
                429 => "Перевищено ліміт запитів. Зачекайте хвилину і спробуйте ще раз.",
                529 => "Сервер Anthropic перевантажений. Спробуйте через кілька хвилин.",
                500 => "Внутрішня помилка сервера Anthropic. Спробуйте пізніше.",

                _ => apiMessage.Contains("credit balance", StringComparison.OrdinalIgnoreCase)
                     ? "На акаунті Anthropic закінчились кредити.\n" +
                       "Поповніть баланс на console.anthropic.com → Billing → Add credits, або введіть ключ іншого акаунту."
                     : string.IsNullOrWhiteSpace(apiMessage)
                       ? $"Помилка API ({code})"
                       : $"Помилка API ({code}): {apiMessage}"
            };

            throw new Exception(userMessage);
        }

        // Розбираємо відповідь
        var responseJson = await response.Content.ReadAsStringAsync();

        string text;
        try
        {
            using var doc = JsonDocument.Parse(responseJson);
            text = doc.RootElement
                      .GetProperty("content")[0]
                      .GetProperty("text")
                      .GetString()
                   ?? throw new Exception("AI повернув порожню відповідь.");
        }
        catch (Exception ex) when (ex is not Exception { Message: { Length: > 0 } })
        {
            throw new Exception($"Помилка розбору відповіді API: {ex.Message}");
        }

        var clean = CleanJsonText(text);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        BackupData backup;
        try
        {
            backup = JsonSerializer.Deserialize<BackupData>(clean, options)
                     ?? throw new Exception("AI повернув некоректний JSON.");
        }
        catch (JsonException)
        {
            // Якщо парсинг провалився — показуємо перші 200 символів для діагностики
            var preview = clean.Length > 200 ? clean[..200] + "..." : clean;
            throw new Exception($"Не вдалося розпарсити відповідь AI. Початок відповіді:\n{preview}");
        }

        backup.AppName = "Study Planner";
        backup.ExportedAt = DateTime.Now.ToString("o");

        return backup;
    }

    //  Промпт
    private static string BuildPrompt(
        AIRecommendationRequest req,
        DateTime today,
        string taskTypes)
    {
        var daysPerTask = req.DifficultyLevel == "Складний" ? 4
                        : req.DifficultyLevel == "Легкий" ? 2 : 3;
        var endDate = today.AddDays(req.TaskCount * daysPerTask);
        var descBlock = string.IsNullOrWhiteSpace(req.SubjectDescription)
            ? "" : $"  - Опис/контекст: {req.SubjectDescription}\n";
        var prefBlock = string.IsNullOrWhiteSpace(req.AdditionalPreferences)
            ? "" : $"  - Додаткові побажання: {req.AdditionalPreferences}\n";

        return $@"Ти — асистент навчального планувальника Study Planner.
Твоє завдання: згенерувати навчальний план у вигляді JSON-бекапу.
 
ПАРАМЕТРИ СТУДЕНТА:
  - Предмет/тема: {req.SubjectName}
{descBlock}  - Кількість завдань: {req.TaskCount}
  - Рівень складності: {req.DifficultyLevel}
  - Типи завдань: {taskTypes}
  - Час навчання: {req.StudyTime}
{prefBlock}
ФОРМАТ ВІДПОВІДІ — ЛИШЕ JSON, без markdown, без коментарів, без пояснень:
{{
  ""Version"": ""1.0"",
  ""AppName"": ""Study Planner"",
  ""ExportedAt"": ""{today:yyyy-MM-ddT00:00:00}"",
  ""UserEmail"": """",
  ""Courses"": [
    {{
      ""OriginalId"": 1,
      ""Name"": ""<назва предмету>"",
      ""Description"": ""<1-2 речення опису>"",
      ""Color"": ""#1c9770"",
      ""CreatedAt"": ""{today:yyyy-MM-ddT00:00:00}"",
      ""Progress"": 0
    }}
  ],
  ""Tasks"": [ ... ],
  ""Statistics"": []
}}
 
ПРАВИЛА:
1. Рівно 1 предмет з назвою: {req.SubjectName}
2. Рівно {req.TaskCount} завдань (всі з OriginalCourseId = 1)
3. Дедлайни рівномірно від {today.AddDays(7):yyyy-MM-dd} до {endDate:yyyy-MM-dd}
4. Priority: 0=Низький, 1=Середній, 2=Високий — розподіли логічно за темою
5. Status завжди 0 (Pending)
6. Відобрази типи завдань «{taskTypes}» у назвах і описах
7. TagsRaw — 1-2 тематичних теги через кому (або порожній рядок)
8. Color — один з: #F05454, #F4845F, #F4A261, #F9C74F, #90BE6D, #52B788, #3AAFA9, #5B8CDB, #7B6CF6, #C77DFF, #F472B6, #E05C97
9. Рівень «{req.DifficultyLevel}»: Легкий → переважно Priority 0-1; Складний → переважно Priority 1-2
10. Завдання мають бути конкретними, корисними, відповідати темі
11. Всі тексти УКРАЇНСЬКОЮ мовою
12. Повертай ТІЛЬКИ JSON — жодного іншого тексту!";
    }

    private static string CleanJsonText(string raw)
    {
        var s = raw.Trim();
        if (s.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
            s = s[7..];
        else if (s.StartsWith("```"))
            s = s[3..];
        if (s.EndsWith("```"))
            s = s[..^3];
        return s.Trim();
    }
}