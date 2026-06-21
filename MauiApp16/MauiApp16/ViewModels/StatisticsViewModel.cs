using System.Collections.ObjectModel;
using System.Windows.Input;
using MauiApp16.Models;
using MauiApp16.Services;

namespace MauiApp16.ViewModels;

public class StatisticsViewModel : BaseViewModel
{
    private readonly IStatisticsService _statisticsService;
    private readonly ITaskService _taskService;
    private readonly IAuthService _authService;
    private readonly ICourseService _courseService;

    private double _completionPercentage;
    public double CompletionPercentage
    {
        get => _completionPercentage;
        set => SetProperty(ref _completionPercentage, value);
    }

    private int _totalTasks;
    public int TotalTasks
    {
        get => _totalTasks;
        set { SetProperty(ref _totalTasks, value); OnPropertyChanged(nameof(PendingTasks)); }
    }

    private int _completedTasks;
    public int CompletedTasks
    {
        get => _completedTasks;
        set { SetProperty(ref _completedTasks, value); OnPropertyChanged(nameof(PendingTasks)); }
    }

    public int PendingTasks => TotalTasks - CompletedTasks;

    private ObservableCollection<ChartDataPoint> _chartData;
    public ObservableCollection<ChartDataPoint> ChartData
    {
        get => _chartData;
        set => SetProperty(ref _chartData, value);
    }

    // Середній час виконання
    private double _avgCompletionHours;
    public double AvgCompletionHours
    {
        get => _avgCompletionHours;
        set
        {
            SetProperty(ref _avgCompletionHours, value);
            OnPropertyChanged(nameof(AvgCompletionDisplay));
        }
    }

    /// <summary>Людиночитабельний рядок: "2 год 30 хв" або "3.5 дн".</summary>
    public string AvgCompletionDisplay
    {
        get
        {
            if (AvgCompletionHours <= 0) return "немає даних";

            if (AvgCompletionHours < 24)
            {
                var h = (int)AvgCompletionHours;
                var m = (int)((AvgCompletionHours - h) * 60);
                return m > 0 ? $"{h} год {m} хв" : $"{h} год";
            }

            var days = Math.Round(AvgCompletionHours / 24, 1);
            return $"{days} дн.";
        }
    }

    // Серія активності
    private int _currentStreak;
    public int CurrentStreak
    {
        get => _currentStreak;
        set => SetProperty(ref _currentStreak, value);
    }

    private int _bestStreak;
    public int BestStreak
    {
        get => _bestStreak;
        set => SetProperty(ref _bestStreak, value);
    }

    // Топ-предмет
    private string _topCourseDisplay;
    public string TopCourseDisplay
    {
        get => _topCourseDisplay;
        set => SetProperty(ref _topCourseDisplay, value);
    }

    // Продуктивність по днях тижня
    private ObservableCollection<WeekDayProductivityItem> _weekDayProductivity;
    public ObservableCollection<WeekDayProductivityItem> WeekDayProductivity
    {
        get => _weekDayProductivity;
        set => SetProperty(ref _weekDayProductivity, value);
    }

    // Розбивка по пріоритетах
    private ObservableCollection<PriorityStatItem> _priorityStats;
    public ObservableCollection<PriorityStatItem> PriorityStats
    {
        get => _priorityStats;
        set => SetProperty(ref _priorityStats, value);
    }

    // ── Конструктор ─────────────────────────────────────────
    public StatisticsViewModel(
        IStatisticsService statisticsService,
        ITaskService taskService,
        IAuthService authService,
        ICourseService courseService)
    {
        _statisticsService = statisticsService;
        _taskService = taskService;
        _authService = authService;
        _courseService = courseService;

        Title = "Статистика";

        ChartData = new ObservableCollection<ChartDataPoint>();
        WeekDayProductivity = new ObservableCollection<WeekDayProductivityItem>();
        PriorityStats = new ObservableCollection<PriorityStatItem>();
        TopCourseDisplay = "—";

        LoadDataCommand = new Command(async () => await LoadDataAsync());
    }

    public ICommand LoadDataCommand { get; }

    public async Task InitializeAsync() => await LoadDataAsync();

    // ── Завантаження даних ──────────────────────────────────
    private async Task LoadDataAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            ChartData.Clear();
            WeekDayProductivity.Clear();
            PriorityStats.Clear();
            CompletionPercentage = 0;
            TotalTasks = 0;
            CompletedTasks = 0;
            AvgCompletionHours = 0;
            CurrentStreak = 0;
            BestStreak = 0;
            TopCourseDisplay = "—";

            // ── Базові показники ──────────────────────────────────────
            CompletionPercentage = await _statisticsService.GetCompletionPercentageAsync();

            var user = await _authService.GetCurrentUserAsync();
            if (user != null)
            {
                var courses = await _courseService.GetAllCoursesAsync();
                var allUserTasks = new List<TaskModel>();
                foreach (var c in courses)
                {
                    var ct = await _taskService.GetTasksByCourseAsync(c.Id);
                    allUserTasks.AddRange(ct);
                }
                TotalTasks = allUserTasks.Count;
                CompletedTasks = allUserTasks.Count(t => t.Status == Models.TaskStatus.Completed);
            }

            // ── Графік 14 днів — ВІДНОСНЕ МАСШТАБУВАННЯ ──────────────
            // Мінімальна ширина для ненульових значень = 8%
            // Максимальне значення = 100%
            const double MinBarPercent = 8.0;

            var chartRaw = await _statisticsService.GetCompletedTasksChartDataAsync(14);
            var chartMax = chartRaw.Values.DefaultIfEmpty(1).Max();
            if (chartMax < 1) chartMax = 1;

            foreach (var kv in chartRaw)
            {
                double barPct = kv.Value == 0
                    ? 0
                    : Math.Max(MinBarPercent, (double)kv.Value / chartMax * 100.0);

                ChartData.Add(new ChartDataPoint
                {
                    Label = kv.Key,
                    Value = kv.Value,
                    BarPercent = Math.Round(barPct, 1),
                });
            }

            // ── Середній час, серії, топ-предмет ─────────────────────
            AvgCompletionHours = await _statisticsService.GetAverageCompletionTimeHoursAsync();
            CurrentStreak = await _statisticsService.GetCurrentStreakAsync();
            BestStreak = await _statisticsService.GetBestStreakAsync();

            var (topName, topCount) = await _statisticsService.GetTopCourseAsync();
            TopCourseDisplay = topCount > 0 ? $"{topName}  ({topCount} ✅)" : "—";

            // ── Продуктивність по днях тижня ─────────────────────────
            var weekRaw = await _statisticsService.GetProductivityByWeekDayAsync();

            var orderedDays = new[]
            {
                (DayOfWeek.Monday,    "Пн"),
                (DayOfWeek.Tuesday,   "Вт"),
                (DayOfWeek.Wednesday, "Ср"),
                (DayOfWeek.Thursday,  "Чт"),
                (DayOfWeek.Friday,    "Пт"),
                (DayOfWeek.Saturday,  "Сб"),
                (DayOfWeek.Sunday,    "Нд"),
            };

            var weekMax = weekRaw.Values.DefaultIfEmpty(1).Max();
            if (weekMax < 1) weekMax = 1;

            var bestDow = weekRaw.OrderByDescending(kv => kv.Value).First().Key;

            foreach (var (dow, name) in orderedDays)
            {
                var count = weekRaw.TryGetValue(dow, out var c) ? c : 0;
                var ratio = (double)count / weekMax;
                double barPct = count == 0
                    ? 0
                    : Math.Max(MinBarPercent, ratio * 100.0);

                WeekDayProductivity.Add(new WeekDayProductivityItem
                {
                    DayName = name,
                    Count = count,
                    BarRatio = ratio,
                    BarPercent = Math.Round(barPct, 1),
                    IsBestDay = (count > 0) && (dow == bestDow),
                });
            }

            // ── Розбивка по пріоритетах ───────────────────────────────
            var prioRaw = await _statisticsService.GetTasksByPriorityAsync();
            var prioTotal = prioRaw.Values.Sum();
            if (prioTotal < 1) prioTotal = 1;

            var prioConfig = new[]
            {
                (TaskPriority.High,   "🔴 Високий",  "#FF5252"),
                (TaskPriority.Medium, "🟡 Середній", "#FFB300"),
                (TaskPriority.Low,    "🟢 Низький",  "#66BB6A"),
            };

            // Для пріоритетів відносна шкала — відносно макс. к-сті
            var prioMax = prioRaw.Values.DefaultIfEmpty(1).Max();
            if (prioMax < 1) prioMax = 1;

            foreach (var (prio, label, color) in prioConfig)
            {
                var cnt = prioRaw.TryGetValue(prio, out var v) ? v : 0;
                var ratio = (double)cnt / prioMax;
                double barPct = cnt == 0
                    ? 0
                    : Math.Max(MinBarPercent, ratio * 100.0);

                PriorityStats.Add(new PriorityStatItem
                {
                    Label = label,
                    Count = cnt,
                    BarRatio = ratio,
                    BarPercent = Math.Round(barPct, 1),
                    BarColor = color,
                });
            }

            OnPropertyChanged(nameof(PendingTasks));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"StatisticsViewModel Error: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Помилка", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}

public class ChartDataPoint
{
    public string Label { get; set; }
    public int Value { get; set; }
    public double BarPercent { get; set; }  // 0..100, відносно макс. за 14 днів
}