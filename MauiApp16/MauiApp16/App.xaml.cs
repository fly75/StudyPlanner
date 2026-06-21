using MauiApp16.Models;
using MauiApp16.Services;
using Plugin.LocalNotification;

namespace MauiApp16;

public partial class App : Application
{
    private readonly IAuthService _authService;
    private readonly ITestDataService _testDataService;
    private readonly ISettingsService _settingsService;
    private readonly MauiApp16.Services.INotificationService _notificationService;

    public App(IAuthService authService,
               ITestDataService testDataService,
               ISettingsService settingsService,
               MauiApp16.Services.INotificationService notificationService,
               ITaskService taskService)
    {
        InitializeComponent();
        _authService = authService;
        _testDataService = testDataService;
        _settingsService = settingsService;
        _notificationService = notificationService;

        // Розриваємо циклічну залежність через setter
        if (notificationService is NotificationService ns)
            ns.SetTaskService(taskService);

        MainPage = new AppShell();
    }

    protected override async void OnStart()
    {
        base.OnStart();

        // Завантажити та застосувати збережену тему
        await InitializeThemeAsync();

        await RequestNotificationPermissionAsync();

        // Створюємо тестовий акаунт при першому запуску
        await InitializeTestAccountAsync();

        var currentUser = await _authService.GetCurrentUserAsync();
        if (currentUser != null)
        {
            // Увімкнути навігаційну панель
            if (MainPage is AppShell shell)
            {
                shell.EnableFlyout();
            }
            await Shell.Current.GoToAsync("//DashboardPage");
        }
        else
        {
            // Вимкнути навігаційну панель
            if (MainPage is AppShell shell)
            {
                shell.DisableFlyout();
            }
            await Shell.Current.GoToAsync("//AuthorizationPage");
        }
    }

    private async Task InitializeThemeAsync()
    {
        try
        {
            var settings = await _settingsService.GetSettingsAsync();
            if (_settingsService is SettingsService settingsService)
            {
                settingsService.ApplyTheme(settings.IsDarkTheme);
            }
        }
        catch (Exception)
        {
            // Ігноруємо помилки ініціалізації теми
        }
    }

    private async Task RequestNotificationPermissionAsync()
    {
        if (OperatingSystem.IsWindows())
            return;

        await LocalNotificationCenter.Current.RequestNotificationPermission();
    }

    private async Task InitializeTestAccountAsync()
    {
        try
        {
            // Перевіряємо чи тестовий акаунт вже існує
            var testEmail = "test.student@gmail.com";
            var testPassword = "123456";

            // Спробуємо увійти, щоб перевірити чи існує акаунт
            var loginSuccess = await _authService.LoginAsync(testEmail, testPassword);

            if (loginSuccess)
            {
                // Акаунт існує, перевіряємо чи є в ньому дані
                var hasData = await _testDataService.HasTestDataAsync();
                if (!hasData)
                {
                    // Додаємо тестові дані якщо їх немає
                    await _testDataService.SeedTestDataAsync();
                }
                // Виходимо з тестового акаунту
                await _authService.LogoutAsync();
            }
            else
            {
                // Створюємо новий тестовий акаунт
                var registerSuccess = await _authService.RegisterAsync(testEmail, testPassword);
                if (registerSuccess)
                {
                    // Додаємо тестові дані
                    await _testDataService.SeedTestDataAsync();
                    // Виходимо з тестового акаунту
                    await _authService.LogoutAsync();
                }
            }
        }
        catch (Exception)
        {
            // Ігноруємо помилки ініціалізації тестового акаунту
        }
    }
}