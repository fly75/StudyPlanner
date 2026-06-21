using System.Collections.ObjectModel;
using System.Windows.Input;
using MauiApp16.Models;
using MauiApp16.Services;

namespace MauiApp16.ViewModels;

public class TasksViewModel : BaseViewModel, IQueryAttributable
{
    private readonly ITaskService _taskService;
    private readonly ICourseService _courseService;
    private readonly IExportService _exportService;

    private int _courseId;
    private Course _selectedCourse;
    private ObservableCollection<TaskModel> _tasks;
    private ObservableCollection<TaskModel> _filteredTasks;

    private string _searchQuery = string.Empty;
    private int _selectedStatusIndex = 0;  // 0=Всі, 1=Pending, 2=InProgress, 3=Completed
    private bool _isFilterExpanded = false;
    private DateTime _deadlineFrom = DateTime.Today;
    private DateTime _deadlineTo = DateTime.Today.AddMonths(1);
    private bool _isDeadlineFilterEnabled = false;  
    private bool _hasActiveFilters = false;

    public TasksViewModel(ITaskService taskService, ICourseService courseService, IExportService exportService)
    {
        _taskService = taskService;
        _courseService = courseService;
        _exportService = exportService;

        Title = "Завдання";
        Tasks = new ObservableCollection<TaskModel>();
        FilteredTasks = new ObservableCollection<TaskModel>();

        LoadTasksCommand = new Command(async () => await LoadTasksAsync());
        AddTaskCommand = new Command(async () => await AddTaskAsync());
        CompleteTaskCommand = new Command<TaskModel>(async (task) => await CompleteTaskAsync(task));
        TaskSelectedCommand = new Command<TaskModel>(async (task) => await OnTaskSelected(task));
        ApplyFiltersCommand = new Command(async () => await ApplyFiltersAsync());
        ClearFiltersCommand = new Command(async () => await ClearFiltersAsync());
        ToggleFilterPanelCommand = new Command(() => IsFilterExpanded = !IsFilterExpanded);

        ExportCsvCommand = new Command(async () => await ExportAsync("csv"));
        ExportJsonCommand = new Command(async () => await ExportAsync("json"));
        ExportTxtCommand = new Command(async () => await ExportAsync("txt"));
    }

    public Course SelectedCourse
    {
        get => _selectedCourse;
        set => SetProperty(ref _selectedCourse, value);
    }

    public ObservableCollection<TaskModel> Tasks
    {
        get => _tasks;
        set => SetProperty(ref _tasks, value);
    }

    public ObservableCollection<TaskModel> FilteredTasks
    {
        get => _filteredTasks;
        set => SetProperty(ref _filteredTasks, value);
    }

    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (SetProperty(ref _searchQuery, value))
                // Пошук спрацьовує одразу при введенні (debounce не потрібен для локальних даних)
                _ = ApplyFiltersAsync();
        }
    }

    public int SelectedStatusIndex
    {
        get => _selectedStatusIndex;
        set => SetProperty(ref _selectedStatusIndex, value);
    }

    public bool IsFilterExpanded
    {
        get => _isFilterExpanded;
        set => SetProperty(ref _isFilterExpanded, value);
    }

    public DateTime DeadlineFrom
    {
        get => _deadlineFrom;
        set => SetProperty(ref _deadlineFrom, value);
    }

    public DateTime DeadlineTo
    {
        get => _deadlineTo;
        set => SetProperty(ref _deadlineTo, value);
    }

    public bool IsDeadlineFilterEnabled
    {
        get => _isDeadlineFilterEnabled;
        set => SetProperty(ref _isDeadlineFilterEnabled, value);
    }

    public bool HasActiveFilters
    {
        get => _hasActiveFilters;
        set => SetProperty(ref _hasActiveFilters, value);
    }

    // --- Нові команди ---
    public ICommand LoadTasksCommand { get; }
    public ICommand AddTaskCommand { get; }
    public ICommand CompleteTaskCommand { get; }
    public ICommand TaskSelectedCommand { get; }
    public ICommand ApplyFiltersCommand { get; }
    public ICommand ClearFiltersCommand { get; }
    public ICommand ToggleFilterPanelCommand { get; }
    public ICommand ExportCsvCommand { get; }
    public ICommand ExportJsonCommand { get; }
    public ICommand ExportTxtCommand { get; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("courseId"))
        {
            _courseId = int.Parse(query["courseId"].ToString());
            Task.Run(async () => await InitializeAsync());
        }
    }

    public async Task InitializeAsync()
    {
        SelectedCourse = await _courseService.GetCourseByIdAsync(_courseId);
        Title = SelectedCourse?.Name ?? "Завдання";
        await LoadTasksAsync();
    }

    public async Task LoadTasksAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var tasks = await _taskService.GetTasksByCourseAsync(_courseId);
            Tasks.Clear();
            foreach (var task in tasks.OrderBy(t => t.Deadline))
                Tasks.Add(task);

            // Після завантаження одразу застосовуємо поточні фільтри
            await ApplyFiltersAsync();
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

    public async Task ApplyFiltersAsync()
    {
        // Конвертуємо індекс Picker у TaskStatus?
        Models.TaskStatus? status = SelectedStatusIndex switch
        {
            1 => Models.TaskStatus.Pending,
            2 => Models.TaskStatus.InProgress,
            3 => Models.TaskStatus.Completed,
            _ => null  // 0 = "Всі"
        };

        var results = await _taskService.SearchTasksAsync(
            _courseId,
            SearchQuery,
            status,
            IsDeadlineFilterEnabled ? DeadlineFrom : null,
            IsDeadlineFilterEnabled ? DeadlineTo : null);

        FilteredTasks.Clear();
        foreach (var task in results.OrderBy(t => t.Deadline))
            FilteredTasks.Add(task);

        // Оновлюємо індикатор активних фільтрів
        HasActiveFilters = !string.IsNullOrWhiteSpace(SearchQuery)
            || SelectedStatusIndex != 0
            || IsDeadlineFilterEnabled;

        IsFilterExpanded = false;
    }

    public async Task ClearFiltersAsync()
    {
        // Скидаємо без тригеру ApplyFilters для кожної зміни
        _searchQuery = string.Empty;
        _selectedStatusIndex = 0;
        _deadlineFrom = DateTime.Today;
        _deadlineTo = DateTime.Today.AddMonths(1);
        _isDeadlineFilterEnabled = false;       

        // Повідомляємо UI про зміну всіх властивостей разом
        OnPropertyChanged(nameof(SearchQuery));
        OnPropertyChanged(nameof(SelectedStatusIndex));
        OnPropertyChanged(nameof(DeadlineFrom));
        OnPropertyChanged(nameof(DeadlineTo));
        OnPropertyChanged(nameof(IsDeadlineFilterEnabled));

        await ApplyFiltersAsync();
        IsFilterExpanded = false;
    }

    private async Task AddTaskAsync()
    {
        await Shell.Current.GoToAsync($"TaskDetailPage?courseId={_courseId}");
    }

    private async Task CompleteTaskAsync(TaskModel task)
    {
        if (task == null) return;
        await _taskService.CompleteTaskAsync(task.Id);
        await LoadTasksAsync();
    }

    private async Task OnTaskSelected(TaskModel task)
    {
        if (task == null) return;
        await Shell.Current.GoToAsync($"TaskDetailPage?taskId={task.Id}");
    }

    private async Task ExportAsync(string format)
    {
        if (FilteredTasks.Count == 0)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Немає даних", "Немає завдань для експорту", "OK");
            return;
        }

        IsBusy = true;
        try
        {
            var tasks = FilteredTasks.ToList();
            var courseName = SelectedCourse?.Name ?? "Всі предмети";
            var safeFileName = $"tasks_{courseName.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}";

            string filePath = format switch
            {
                "csv" => await _exportService.ExportToCsvAsync(tasks, safeFileName),
                "json" => await _exportService.ExportToJsonAsync(tasks, safeFileName),
                "txt" => await _exportService.ExportToTxtAsync(tasks, safeFileName, courseName),
                _ => throw new ArgumentException("Невідомий формат")
            };

            await _exportService.ShareFileAsync(filePath, $"Експорт — {courseName}");
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
}