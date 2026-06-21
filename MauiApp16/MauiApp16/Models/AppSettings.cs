namespace MauiApp16.Models;

public class AppSettings
{
    public bool IsDarkTheme { get; set; }
    public bool NotificationsEnabled { get; set; }

    // Зберігаємо в хвилинах: 7 днів=10080, 3 дні=4320, 1 день=1440,
    // 12 год=720, 3 год=180, 1 год=60
    public int ReminderMinutesBefore { get; set; } = 60;
}