using System.Windows.Input;
using MauiApp16.Models;
using MauiApp16.Services;

namespace MauiApp16.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private readonly ISettingsService _settingsService;
    private readonly IAuthService _authService;
    private readonly ITaskService _taskService;
    private readonly INotificationService _notificationService;
    private readonly IBackupService _backupService;

    private AppSettings _settings;
    private string _lastBackupInfo = "Бекап не створювався";

    private string _currentPassword;
    private string _newPassword;
    private string _confirmNewPassword;
    private string _passwordChangeMessage;
    private bool _passwordChangeSuccess;

    public SettingsViewModel(
        ISettingsService settingsService,
        IAuthService authService,
        ITaskService taskService,
        INotificationService notificationService,
        IBackupService backupService)
    {
        _settingsService = settingsService;
        _authService = authService;
        _taskService = taskService;
        _notificationService = notificationService;
        _backupService = backupService;

        Title = "Налаштування";

        SaveCommand = new Command(async () => await SaveAsync());
        LogoutCommand = new Command(async () => await LogoutAsync());
        DeleteAccountCommand = new Command(async () => await DeleteAccountAsync());

        ExportBackupCommand = new Command(async () => await ExportBackupAsync());
        ImportBackupCommand = new Command(async () => await ImportBackupAsync());
        ChangePasswordCommand = new Command(async () => await ChangePasswordAsync());

        MergeImportBackupCommand = new Command(async () =>
        {
            IsBusy = true;
            try
            {
                var (success, message) = await _backupService.MergeImportBackupAsync();
                await Application.Current.MainPage.DisplayAlert(
                    success ? "✅ Злиття успішне" : "❌ Помилка",
                    message,
                    "OK");

                if (success)
                    await LoadLastBackupInfoAsync();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Помилка", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }, () => !IsBusy);

        ImportSingleTaskCommand = new Command(async () =>
        {
            IsBusy = true;
            try
            {
                var (success, message) = await _backupService.ImportSingleTaskAsync();
                await Application.Current.MainPage.DisplayAlert(
                    success ? "✅ Імпорт успішний" : "❌ Помилка",
                    message,
                    "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Помилка", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }, () => !IsBusy);
    }

    public AppSettings Settings
    {
        get => _settings;
        set => SetProperty(ref _settings, value);
    }

    public string LastBackupInfo
    {
        get => _lastBackupInfo;
        set => SetProperty(ref _lastBackupInfo, value);
    }

    public string CurrentPassword
    {
        get => _currentPassword;
        set => SetProperty(ref _currentPassword, value);
    }
    public string NewPassword
    {
        get => _newPassword;
        set => SetProperty(ref _newPassword, value);
    }
    public string ConfirmNewPassword
    {
        get => _confirmNewPassword;
        set => SetProperty(ref _confirmNewPassword, value);
    }
    public string PasswordChangeMessage
    {
        get => _passwordChangeMessage;
        set => SetProperty(ref _passwordChangeMessage, value);
    }
    public bool PasswordChangeSuccess
    {
        get => _passwordChangeSuccess;
        set => SetProperty(ref _passwordChangeSuccess, value);
    }

    private int _reminderIndex = 2; // за замовчуванням "За 1 день"

    public int ReminderIndex
    {
        get => _reminderIndex;
        set
        {
            if (SetProperty(ref _reminderIndex, value))
            {
                if (_settings != null)
                    _settings.ReminderMinutesBefore = value switch
                    {
                        0 => 10080, // За 7 днів
                        1 => 4320,  // За 3 дні
                        2 => 1440,  // За 1 день
                        3 => 720,   // За 12 годин
                        4 => 180,   // За 3 години
                        5 => 60,    // За 1 годину
                        _ => 1440
                    };
            }
        }
    }

    public async Task InitializeAsync()
    {
        Settings = await _settingsService.GetSettingsAsync();

        await LoadLastBackupInfoAsync();

        ReminderIndex = Settings.ReminderMinutesBefore switch
        {
            10080 => 0,
            4320 => 1,
            1440 => 2,
            720 => 3,
            180 => 4,
            60 => 5,
            _ => 2
        };
    }

    public ICommand SaveCommand { get; }
    public ICommand LogoutCommand { get; }
    public ICommand DeleteAccountCommand { get; }
    public ICommand ExportBackupCommand { get; }
    public ICommand ImportBackupCommand { get; }
    public ICommand ChangePasswordCommand { get; }
    public ICommand MergeImportBackupCommand { get; }
    public ICommand ImportSingleTaskCommand { get; }

    private async Task SaveAsync()
    {
        IsBusy = true;
        try
        {
            await _settingsService.SaveSettingsAsync(Settings);
            await RescheduleAllNotificationsAsync();
            await Application.Current.MainPage.DisplayAlert("Успіх", "Налаштування збережено", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Помилка", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RescheduleAllNotificationsAsync()
    {
        var tasks = await _taskService.GetAllTasksAsync();
        var settings = await _settingsService.GetSettingsAsync();

        foreach (var task in tasks.Where(t => t.Status != Models.TaskStatus.Completed))
        {
            await _notificationService.CancelNotificationAsync(task.Id);

            if (settings.NotificationsEnabled)
            {
                var notifyTime = task.Deadline.AddMinutes(-settings.ReminderMinutesBefore);
                await _notificationService.ScheduleNotificationAsync(
                    task.Id,
                    task.Title,
                    $"Дедлайн: {task.Deadline:dd.MM.yyyy HH:mm}",
                    notifyTime);
            }
        }
    }

    private async Task LogoutAsync()
    {
        bool confirm = await Application.Current.MainPage.DisplayAlert(
            "Вийти?", "Ви впевнені, що хочете вийти?", "Так", "Ні");

        if (!confirm) return;

        await _authService.LogoutAsync();

        if (Application.Current.MainPage is AppShell shell)
            shell.DisableFlyout();

        await Shell.Current.GoToAsync("//AuthorizationPage");
    }

    private async Task DeleteAccountAsync()
    {
        bool confirm = await Application.Current.MainPage.DisplayAlert(
            "Видалити акаунт?",
            "Це видалить ваш акаунт та ВСІ дані назавжди. Цю дію неможливо скасувати!",
            "Видалити", "Скасувати");

        if (!confirm) return;

        bool doubleConfirm = await Application.Current.MainPage.DisplayAlert(
            "Остаточне підтвердження",
            "Ви впевнені? Всі ваші предмети, завдання та статистика будуть видалені назавжди.",
            "Так, видалити", "Ні");

        if (!doubleConfirm) return;

        IsBusy = true;
        try
        {
            var success = await _authService.DeleteAccountAsync();
            if (success)
            {
                await Application.Current.MainPage.DisplayAlert("Успіх", "Акаунт успішно видалено", "OK");

                if (Application.Current.MainPage is AppShell shell)
                    shell.DisableFlyout();

                await Shell.Current.GoToAsync("//AuthorizationPage");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Помилка", "Не вдалося видалити акаунт", "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Помилка", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadLastBackupInfoAsync()
    {
        try
        {
            var (date, size) = await _backupService.GetLastBackupInfoAsync();
            if (date == null)
            {
                LastBackupInfo = "Бекап не створювався";
                return;
            }

            var kb = size / 1024.0;
            LastBackupInfo = $"Останній: {date:dd.MM.yyyy HH:mm}  •  {kb:F1} КБ";
        }
        catch
        {
            LastBackupInfo = "Інформація недоступна";
        }
    }

    private async Task ExportBackupAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            await _backupService.ExportBackupAsync();
            await LoadLastBackupInfoAsync();
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Помилка", ex.Message, "OK");
        }
        finally { IsBusy = false; }
    }

    private async Task ImportBackupAsync()
    {
        if (IsBusy) return;

        bool confirm = await Application.Current.MainPage.DisplayAlert(
            "Відновити з бекапу?",
            "Усі поточні предмети, завдання та статистика будуть ЗАМІНЕНІ даними з файлу бекапу. Продовжити?",
            "Так, відновити",
            "Скасувати");

        if (!confirm) return;

        IsBusy = true;
        try
        {
            var (success, message) = await _backupService.ImportBackupAsync();

            if (success)
                await Application.Current.MainPage.DisplayAlert("✅ Успішно", message, "OK");
            else if (!string.IsNullOrEmpty(message))
                await Application.Current.MainPage.DisplayAlert("Помилка", message, "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Помилка", ex.Message, "OK");
        }
        finally { IsBusy = false; }
    }

    private async Task ChangePasswordAsync()
    {
        if (string.IsNullOrWhiteSpace(_currentPassword) ||
            string.IsNullOrWhiteSpace(_newPassword))
        {
            _passwordChangeMessage = "Заповніть всі поля";
            _passwordChangeSuccess = false;
            return;
        }
        if (_newPassword != _confirmNewPassword)
        {
            _passwordChangeMessage = "Нові паролі не співпадають";
            _passwordChangeSuccess = false;
            return;
        }
        if (_newPassword.Length < 6)
        {
            _passwordChangeMessage = "Пароль має бути не менше 6 символів";
            _passwordChangeSuccess = false;
            return;
        }

        var ok = await _authService.ChangePasswordAsync(_currentPassword, _newPassword);
        if (ok)
        {
            _passwordChangeMessage = "✅ Пароль успішно змінено";
            _passwordChangeSuccess = true;
            _currentPassword = _newPassword = _confirmNewPassword = string.Empty;
        }
        else
        {
            _passwordChangeMessage = "❌ Невірний поточний пароль";
            _passwordChangeSuccess = false;
        }
    }
}