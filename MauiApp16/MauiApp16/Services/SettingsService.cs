using MauiApp16.Models;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace MauiApp16.Services;

public class SettingsService : ISettingsService
{
    private readonly ISecureStorageService _secureStorage;
    private AppSettings _cachedSettings;

    public SettingsService(ISecureStorageService secureStorage)
    {
        _secureStorage = secureStorage;
    }

    public async Task<AppSettings> GetSettingsAsync()
    {
        if (_cachedSettings != null)
            return _cachedSettings;

        var isDarkTheme = await _secureStorage.GetAsync("IsDarkTheme");
        var notificationsEnabled = await _secureStorage.GetAsync("NotificationsEnabled");
        var reminderMinutes = await _secureStorage.GetAsync("ReminderMinutesBefore");

        _cachedSettings = new AppSettings
        {
            IsDarkTheme = isDarkTheme == "true",
            NotificationsEnabled = notificationsEnabled != "false",
            ReminderMinutesBefore = int.TryParse(reminderMinutes, out var mins) ? mins : 60
        };

        return _cachedSettings;
    }

    public async Task SaveSettingsAsync(AppSettings settings)
    {
        _cachedSettings = settings;
        await _secureStorage.SetAsync("IsDarkTheme", settings.IsDarkTheme.ToString().ToLower());
        await _secureStorage.SetAsync("NotificationsEnabled", settings.NotificationsEnabled.ToString().ToLower());
        await _secureStorage.SetAsync("ReminderMinutesBefore", settings.ReminderMinutesBefore.ToString());

        // Застосувати тему негайно
        ApplyTheme(settings.IsDarkTheme);
    }

    public void ApplyTheme(bool isDarkTheme)
    {
        if (Application.Current?.Resources == null) return;

        // Оновити динамічні кольори
        if (isDarkTheme)
        {
            // Темна тема
            Application.Current.Resources["Primary"] = Application.Current.Resources["PrimaryDark"];
            Application.Current.Resources["Secondary"] = Application.Current.Resources["SecondaryDark"];
            Application.Current.Resources["Tertiary"] = Application.Current.Resources["TertiaryDark"];
            Application.Current.Resources["Accent"] = Application.Current.Resources["AccentDark"];
            Application.Current.Resources["Background"] = Application.Current.Resources["BackgroundDark"];
            Application.Current.Resources["Surface"] = Application.Current.Resources["SurfaceDark"];
            Application.Current.Resources["TextPrimary"] = Application.Current.Resources["TextDark"];
            Application.Current.Resources["TextSecondary"] = Application.Current.Resources["TextSecondaryDark"];
            Application.Current.Resources["ProgressBackground"] = Application.Current.Resources["ProgressBackgroundDark"];

            // Додатково оновлюємо Brushes
            Application.Current.Resources["BackgroundBrush"] = Application.Current.Resources["BackgroundDarkBrush"];
            Application.Current.Resources["SurfaceBrush"] = Application.Current.Resources["SurfaceDarkBrush"];
        }
        else
        {
            // Світла тема
            Application.Current.Resources["Primary"] = Application.Current.Resources["PrimaryLight"];
            Application.Current.Resources["Secondary"] = Application.Current.Resources["SecondaryLight"];
            Application.Current.Resources["Tertiary"] = Application.Current.Resources["TertiaryLight"];
            Application.Current.Resources["Accent"] = Application.Current.Resources["AccentLight"];
            Application.Current.Resources["Background"] = Application.Current.Resources["BackgroundLight"];
            Application.Current.Resources["Surface"] = Application.Current.Resources["SurfaceLight"];
            Application.Current.Resources["TextPrimary"] = Application.Current.Resources["TextLight"];
            Application.Current.Resources["TextSecondary"] = Application.Current.Resources["TextSecondaryLight"];
            Application.Current.Resources["ProgressBackground"] = Application.Current.Resources["ProgressBackgroundLight"];

            // Додатково оновлюємо Brushes
            Application.Current.Resources["BackgroundBrush"] = Application.Current.Resources["BackgroundLightBrush"];
            Application.Current.Resources["SurfaceBrush"] = Application.Current.Resources["SurfaceLightBrush"];
        }

        Application.Current.Resources["QrCodeIcon"] = isDarkTheme
                ? "qr_code.png"
                : "qr_code_dark.png";

        // Оновити колір фону всього додатку
        if (Application.Current.MainPage != null)
        {
            Application.Current.MainPage.BackgroundColor = (Color)Application.Current.Resources["Background"];

            // Оновити AppShell кольори
            if (Application.Current.MainPage is AppShell shell)
            {
                shell.UpdateThemeColors();
            }
        }

        // Примусово оновити всі сторінки
        RefreshAllPages();
    }

    private void RefreshAllPages()
    {
        if (Application.Current?.MainPage == null) return;

        try
        {
            // Рекурсивно оновлюємо всі елементи
            UpdateElementColors(Application.Current.MainPage);
        }
        catch (Exception)
        {
            // Ігноруємо помилки оновлення
        }
    }

    private void UpdateElementColors(Element element)
    {
        if (element == null) return;

        // Оновлюємо колір фону для сторінок
        if (element is ContentPage page)
        {
            page.BackgroundColor = (Color)Application.Current.Resources["Background"];
        }

        // Оновлюємо Frame
        if (element is Frame frame)
        {
            if (frame.BackgroundColor == Colors.Transparent ||
                frame.BackgroundColor.ToHex().Contains("F0F9F7") ||
                frame.BackgroundColor.ToHex().Contains("142828"))
            {
                frame.BackgroundColor = (Color)Application.Current.Resources["Surface"];
            }
        }

        // Оновлюємо Label
        if (element is Label label)
        {
            // Перевіряємо чи це не особливий колір (наприклад, для помилок чи акцентів)
            var currentColor = label.TextColor;
            if (currentColor != null)
            {
                var hex = currentColor.ToHex();
                // Якщо це звичайний текст (чорний або білий відтінок)
                if (hex.Contains("212121") || hex.Contains("E0E0E0") || hex.Contains("616161") || hex.Contains("BDBDBD"))
                {
                    // Визначаємо чи це primary чи secondary текст
                    if (hex.Contains("616161") || hex.Contains("BDBDBD"))
                    {
                        label.TextColor = (Color)Application.Current.Resources["TextSecondary"];
                    }
                    else
                    {
                        label.TextColor = (Color)Application.Current.Resources["TextPrimary"];
                    }
                }
            }
        }

        // Рекурсивно обробляємо дочірні елементи
        if (element is Layout layout)
        {
            foreach (var child in layout.Children)
            {
                if (child is Element childElement)
                {
                    UpdateElementColors(childElement);
                }
            }
        }
        else if (element is ContentView contentView && contentView.Content != null)
        {
            UpdateElementColors(contentView.Content);
        }
        else if (element is ScrollView scrollView && scrollView.Content != null)
        {
            UpdateElementColors(scrollView.Content);
        }
    }
}