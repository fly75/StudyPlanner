namespace MauiApp16.Services;

public class MotivationService : IMotivationService
{
    private readonly string[] _messages = new[]
    {
        "Чудова робота! Продовжуй у тому ж дусі!",
        "Кожен крок наближає тебе до мети!",
        "Ти робиш успіхи! Так тримати!",
        "Навчання - це інвестиція в майбутнє!",
        "Вперед до нових знань і досягнень!",
        "Твоя наполегливість приносить результат!",
        "Успіх складається з маленьких кроків!"
    };

    public Task<string> GetMotivationalMessageAsync()
    {
        var random = new Random();
        var message = _messages[random.Next(_messages.Length)];
        return Task.FromResult(message);
    }
}