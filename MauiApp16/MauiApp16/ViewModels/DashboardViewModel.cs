using System.Collections.ObjectModel;
using System.Windows.Input;
using MauiApp16.Models;
using MauiApp16.Services;

namespace MauiApp16.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    private readonly ITaskService _taskService;
    private readonly IStatisticsService _statisticsService;
    private readonly IMotivationService _motivationService;
    private readonly INotificationService _notificationService;
    private readonly IAuthService _authService;
    private readonly IInAppNotificationService _inAppNotificationService;

    private string _motivationalMessage;
    private double _overallProgress;
    private ObservableCollection<TaskModel> _upcomingTasks;

    private string _userDisplayName;
    private ImageSource _avatarSource;

    private bool _isNotificationPanelVisible;
    private bool _isUserMenuVisible;
    private bool _showUnreadOnly;
    private int _unreadNotificationCount;

    private ObservableCollection<AppNotification> _allNotifications = new();
    private ObservableCollection<AppNotification> _notifications = new();

    public DashboardViewModel(
        ITaskService taskService,
        IStatisticsService statisticsService,
        IMotivationService motivationService,
        INotificationService notificationService,
        IAuthService authService,
        IInAppNotificationService inAppNotificationService)
    {
        _taskService = taskService;
        _statisticsService = statisticsService;
        _motivationService = motivationService;
        _notificationService = notificationService;
        _authService = authService;
        _inAppNotificationService = inAppNotificationService;

        Title = "Головна";
        UpcomingTasks = new ObservableCollection<TaskModel>();
        Notifications = new ObservableCollection<AppNotification>();

        // Існуючі команди
        GoToCoursesCommand = new Command(async () => await Shell.Current.GoToAsync("CoursesPage"));
        QuickAddTaskCommand = new Command(async () => await Shell.Current.GoToAsync("TaskDetailPage"));

        // Нові команди — сповіщення
        ToggleNotificationPanelCommand = new Command(async () => await ToggleNotificationPanelAsync());
        CloseNotificationPanelCommand = new Command(() => IsNotificationPanelVisible = false);
        MarkNotificationReadCommand = new Command<AppNotification>(async n => await MarkReadAsync(n));

        // Нові команди — меню користувача
        ToggleUserMenuCommand = new Command(() => IsUserMenuVisible = !IsUserMenuVisible);
        CloseUserMenuCommand = new Command(() => IsUserMenuVisible = false);
        GoToProfileCommand = new Command(async () =>
        {
            IsUserMenuVisible = false;
            await Shell.Current.GoToAsync("ProfilePage");
        });
        GoToSettingsCommand = new Command(async () =>
        {
            IsUserMenuVisible = false;
            await Shell.Current.GoToAsync("//SettingsPage");
        });
        LogoutCommand = new Command(async () => await LogoutAsync());

    }

    public string MotivationalMessage
    {
        get => _motivationalMessage;
        set => SetProperty(ref _motivationalMessage, value);
    }
    public double OverallProgress
    {
        get => _overallProgress;
        set => SetProperty(ref _overallProgress, value);
    }
    public ObservableCollection<TaskModel> UpcomingTasks
    {
        get => _upcomingTasks;
        set => SetProperty(ref _upcomingTasks, value);
    }

    public string UserDisplayName
    {
        get => _userDisplayName;
        set => SetProperty(ref _userDisplayName, value);
    }
    public ImageSource AvatarSource
    {
        get => _avatarSource;
        set => SetProperty(ref _avatarSource, value);
    }
    public bool IsUserMenuVisible
    {
        get => _isUserMenuVisible;
        set => SetProperty(ref _isUserMenuVisible, value);
    }

    public bool IsNotificationPanelVisible
    {
        get => _isNotificationPanelVisible;
        set => SetProperty(ref _isNotificationPanelVisible, value);
    }
    public int UnreadNotificationCount
    {
        get => _unreadNotificationCount;
        set
        {
            if (SetProperty(ref _unreadNotificationCount, value))
                OnPropertyChanged(nameof(HasUnreadNotifications));
        }
    }
    public bool HasUnreadNotifications => _unreadNotificationCount > 0;

    public bool ShowUnreadOnly
    {
        get => _showUnreadOnly;
        set
        {
            if (SetProperty(ref _showUnreadOnly, value))
                ApplyNotificationFilter();
        }
    }
    public ObservableCollection<AppNotification> Notifications
    {
        get => _notifications;
        set => SetProperty(ref _notifications, value);
    }

    // Команди

    public ICommand GoToCoursesCommand { get; }
    public ICommand QuickAddTaskCommand { get; }
    public ICommand ToggleNotificationPanelCommand { get; }
    public ICommand CloseNotificationPanelCommand { get; }
    public ICommand MarkNotificationReadCommand { get; }
    public ICommand ToggleUserMenuCommand { get; }
    public ICommand CloseUserMenuCommand { get; }
    public ICommand GoToProfileCommand { get; }
    public ICommand GoToSettingsCommand { get; }
    public ICommand LogoutCommand { get; }

    // Методи

    public async Task InitializeAsync() => await LoadDataAsync();

    private async Task LoadDataAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            // Очищаємо
            UpcomingTasks.Clear();
            MotivationalMessage = string.Empty;
            OverallProgress = 0;

            // Дані користувача
            var user = await _authService.GetCurrentUserAsync();
            if (user != null)
            {
                UserDisplayName = user.DisplayName;
                AvatarSource = !string.IsNullOrEmpty(user.AvatarPath)
                    ? ImageSource.FromFile(user.AvatarPath)
                    : ImageSource.FromFile("user_placeholder.png");
            }

            // Мотивація і прогрес
            MotivationalMessage = await _motivationService.GetMotivationalMessageAsync();
            OverallProgress = await _statisticsService.GetCompletionPercentageAsync();

            // Найближчі завдання
            var tasks = await _taskService.GetUpcomingTasksAsync(7);
            foreach (var task in tasks.OrderBy(t => t.Deadline).Take(5))
                UpcomingTasks.Add(task);

            // Push-нотифікації для сьогоднішніх дедлайнів (+ in-app)
            var allTasks = await _taskService.GetAllTasksAsync();
            await _notificationService.NotifyTodayDeadlinesAsync(allTasks);

            // Додаємо in-app нотифікації для сьогоднішніх дедлайнів
            var todayTasks = allTasks.Where(t =>
                t.Status != Models.TaskStatus.Completed &&
                t.Deadline.Date == DateTime.Today).ToList();
            foreach (var t in todayTasks)
                await _inAppNotificationService.AddNotificationAsync(
                    $"⏰ Сьогодні дедлайн!",
                    $"📌 {t.Title}\n📚 {t.CourseName}\n🗓 {t.Deadline:HH:mm}",
                    t.Id);

            // Кількість непрочитаних
            UnreadNotificationCount = await _inAppNotificationService.GetUnreadCountAsync();
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Помилка", ex.Message, "OK");
        }
        finally { IsBusy = false; }
    }

    private async Task ToggleNotificationPanelAsync()
    {
        if (!IsNotificationPanelVisible)
        {
            // Завантажуємо список
            var list = await _inAppNotificationService.GetNotificationsAsync(_showUnreadOnly);
            _allNotifications.Clear();
            foreach (var n in list) _allNotifications.Add(n);
            ApplyNotificationFilter();
        }
        IsNotificationPanelVisible = !IsNotificationPanelVisible;
        IsUserMenuVisible = false; // Закриваємо меню користувача
    }

    private void ApplyNotificationFilter()
    {
        Notifications.Clear();
        var source = _showUnreadOnly
            ? _allNotifications.Where(n => !n.IsRead)
            : _allNotifications.AsEnumerable();
        foreach (var n in source) Notifications.Add(n);
    }

    private async Task MarkReadAsync(AppNotification notification)
    {
        if (notification == null || notification.IsRead) return;
        await _inAppNotificationService.MarkAsReadAsync(notification.Id);
        notification.IsRead = true;
        // Оновлюємо лічильник і список
        UnreadNotificationCount = Math.Max(0, UnreadNotificationCount - 1);
        ApplyNotificationFilter();
    }

    private async Task LogoutAsync()
    {
        IsUserMenuVisible = false;
        bool confirm = await Application.Current.MainPage.DisplayAlert(
            "Вихід", "Ви дійсно хочете вийти?", "Так", "Ні");
        if (!confirm) return;

        // authService.LogoutAsync() — вже є в SettingsViewModel, тут теж викликаємо
        // Оскільки IAuthService доступний, просто навігуємося до AuthPage
        await (Application.Current?.Handler?.MauiContext?.Services
            .GetService(typeof(IAuthService)) as IAuthService)?.LogoutAsync();

        if (Application.Current.MainPage is AppShell shell)
            shell.DisableFlyout();

        await Shell.Current.GoToAsync("//AuthorizationPage");
    }
}