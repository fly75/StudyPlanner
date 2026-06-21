using System.Collections.ObjectModel;
using System.Windows.Input;
using MauiApp16.Models;
using MauiApp16.Services;

namespace MauiApp16.ViewModels;

public class CalendarViewModel : BaseViewModel
{
    private readonly ITaskService _taskService;

    private DateTime _currentWeekStart;
    private ObservableCollection<WeekDay> _weekDays;
    private string _weekRangeTitle;
    private TaskModel _selectedTask;

    public CalendarViewModel(ITaskService taskService)
    {
        _taskService = taskService;
        Title = "Календар";

        // Починаємо з поточного тижня (понеділок)
        _currentWeekStart = GetMonday(DateTime.Today);

        WeekDays = new ObservableCollection<WeekDay>();

        LoadWeekCommand = new Command(async () => await LoadWeekAsync());
        PreviousWeekCommand = new Command(async () => await ChangeWeekAsync(-7));
        NextWeekCommand = new Command(async () => await ChangeWeekAsync(7));
        TodayCommand = new Command(async () => await GoToTodayAsync());
        TaskSelectedCommand = new Command<TaskModel>(async t => await OnTaskSelectedAsync(t));
    }

    public ObservableCollection<WeekDay> WeekDays
    {
        get => _weekDays;
        set => SetProperty(ref _weekDays, value);
    }

    public string WeekRangeTitle
    {
        get => _weekRangeTitle;
        set => SetProperty(ref _weekRangeTitle, value);
    }

    public ICommand LoadWeekCommand { get; }
    public ICommand PreviousWeekCommand { get; }
    public ICommand NextWeekCommand { get; }
    public ICommand TodayCommand { get; }
    public ICommand TaskSelectedCommand { get; }

    public async Task InitializeAsync()
        => await LoadWeekAsync();

    private async Task LoadWeekAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var weekEnd = _currentWeekStart.AddDays(6);
            WeekRangeTitle = $"{_currentWeekStart:dd.MM} – {weekEnd:dd.MM.yyyy}";

            // 👇 Беремо ВСІ завдання — і минулі і майбутні
            var allTasks = await _taskService.GetAllTasksAsync();

            WeekDays.Clear();
            var dayNames = new[] { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Нд" };

            for (int i = 0; i < 7; i++)
            {
                var date = _currentWeekStart.AddDays(i);
                var dayTasks = allTasks
                    .Where(t => t.Deadline.Date == date.Date)
                    .OrderBy(t => t.Deadline)
                    .ToList();

                WeekDays.Add(new WeekDay
                {
                    Date = date,
                    DayName = dayNames[i],
                    DayNumber = date.Day.ToString(),
                    Tasks = dayTasks
                });
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage
                .DisplayAlert("Помилка", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ChangeWeekAsync(int days)
    {
        _currentWeekStart = _currentWeekStart.AddDays(days);
        await LoadWeekAsync();
    }

    private async Task GoToTodayAsync()
    {
        _currentWeekStart = GetMonday(DateTime.Today);
        await LoadWeekAsync();
    }

    private async Task OnTaskSelectedAsync(TaskModel task)
    {
        if (task == null) return;
        await Shell.Current.GoToAsync($"TaskDetailPage?taskId={task.Id}");
    }

    private static DateTime GetMonday(DateTime date)
    {
        // DayOfWeek: Sunday=0, Monday=1 ... тому коригуємо під понеділок
        int diff = ((int)date.DayOfWeek - 1 + 7) % 7;
        return date.AddDays(-diff).Date;
    }
}
