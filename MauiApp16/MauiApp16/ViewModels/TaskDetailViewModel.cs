using System.Collections.ObjectModel;
using System.Windows.Input;
using MauiApp16.Models;
using MauiApp16.Services;

namespace MauiApp16.ViewModels;

public class TaskDetailViewModel : BaseViewModel, IQueryAttributable
{
    private readonly ITaskService _taskService;
    private readonly ICourseService _courseService;

    private int _taskId;
    private int _courseId;
    private TaskModel? _task;
    private bool _isNewTask;
    private ObservableCollection<TagItem> _availableTags;

    private static readonly List<TagItem> DefaultTags = new()
    {
        new TagItem { Name = "іспит",        Emoji = "📝" },
        new TagItem { Name = "лабораторна",  Emoji = "🔬" },
        new TagItem { Name = "реферат",      Emoji = "📄" },
        new TagItem { Name = "курсова",      Emoji = "📘" },
        new TagItem { Name = "домашня",      Emoji = "🏠" },
        new TagItem { Name = "практична",    Emoji = "⚙️" },
        new TagItem { Name = "семінар",      Emoji = "🎓" },
        new TagItem { Name = "проект",       Emoji = "🚀" },
    };

    public TaskDetailViewModel(ITaskService taskService, ICourseService courseService)
    {
        _taskService = taskService;
        _courseService = courseService;

        Title = "Деталі завдання";

        _availableTags = new ObservableCollection<TagItem>();

        SaveCommand = new Command(async () => await SaveAsync());
        DeleteCommand = new Command(async () => await DeleteAsync());
        ToggleTagCommand = new Command<TagItem>(OnToggleTag);
    }

    public TaskModel? Task
    {
        get => _task;
        set => SetProperty(ref _task, value);
    }

    public bool IsNewTask
    {
        get => _isNewTask;
        set => SetProperty(ref _isNewTask, value);
    }

    public ObservableCollection<TagItem> AvailableTags
    {
        get => _availableTags;
        set => SetProperty(ref _availableTags, value);
    }

    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand ToggleTagCommand { get; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("taskId"))
        {
            _taskId = int.Parse(query["taskId"].ToString()!);
            IsNewTask = false;
        }
        else if (query.ContainsKey("courseId"))
        {
            _courseId = int.Parse(query["courseId"].ToString()!);
            IsNewTask = true;
        }

        System.Threading.Tasks.Task.Run(async () => await InitializeAsync());
    }

    public async System.Threading.Tasks.Task InitializeAsync()
    {
        if (_taskId > 0)
        {
            Task = await _taskService.GetTaskByIdAsync(_taskId);
            Title = "Редагувати завдання";
        }
        else
        {
            Task = new TaskModel
            {
                CourseId = _courseId,
                Deadline = DateTime.Now.AddDays(7),
                Priority = TaskPriority.Low,
                Status = Models.TaskStatus.Pending
            };
            Title = "Нове завдання";
        }

        await MainThread.InvokeOnMainThreadAsync(() => InitializeTags());
    }

    private void InitializeTags()
    {
        if (_availableTags == null)
            _availableTags = new ObservableCollection<TagItem>();

        AvailableTags.Clear();
        var selectedTags = Task?.Tags ?? new List<string>();

        foreach (var tag in DefaultTags)
        {
            AvailableTags.Add(new TagItem
            {
                Name = tag.Name,
                Emoji = tag.Emoji,
                IsSelected = selectedTags.Contains(tag.Name)
            });
        }
    }

    private void OnToggleTag(TagItem tag)
    {
        if (tag == null || Task == null) return;

        tag.IsSelected = !tag.IsSelected;

        // Оновлюємо Tags у Task
        var currentTags = Task.Tags;
        if (tag.IsSelected)
        {
            if (!currentTags.Contains(tag.Name))
                currentTags.Add(tag.Name);
        }
        else
        {
            currentTags.Remove(tag.Name);
        }

        Task.Tags = currentTags;

        // Оновлюємо UI — CollectionView не стежить за IsSelected автоматично
        var index = AvailableTags.IndexOf(tag);
        if (index >= 0)
        {
            AvailableTags.RemoveAt(index);
            AvailableTags.Insert(index, tag);
        }
    }

    private async System.Threading.Tasks.Task SaveAsync()
    {
        if (Task == null || string.IsNullOrWhiteSpace(Task.Title))
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert("Помилка", "Введіть назву завдання", "OK");
            }
            return;
        }

        IsBusy = true;

        try
        {
            await _taskService.SaveTaskAsync(Task);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert("Помилка", ex.Message, "OK");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async System.Threading.Tasks.Task DeleteAsync()
    {
        if (Application.Current?.MainPage == null || Task == null)
            return;

        bool confirm = await Application.Current.MainPage.DisplayAlert(
            "Видалити завдання?",
            "Ви впевнені?",
            "Видалити",
            "Скасувати");

        if (!confirm)
            return;

        IsBusy = true;

        try
        {
            await _taskService.DeleteTaskAsync(Task.Id);
            await Shell.Current.GoToAsync("..");
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