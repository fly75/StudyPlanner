using System.Collections.ObjectModel;
using System.Windows.Input;
using MauiApp16.Models;
using MauiApp16.Services;

namespace MauiApp16.ViewModels;

public class CoursesViewModel : BaseViewModel
{
    private readonly ICourseService _courseService;
    private ObservableCollection<Course> _courses;
    private ObservableCollection<Course> _filteredCourses;
    private string _searchQuery = string.Empty;

    public CoursesViewModel(ICourseService courseService)
    {
        _courseService = courseService;
        Title = "Предмети";
        Courses = new ObservableCollection<Course>();
        FilteredCourses = new ObservableCollection<Course>();

        LoadCoursesCommand = new Command(async () => await LoadCoursesAsync());
        AddCourseCommand = new Command(async () => await AddCourseAsync());
        DeleteCourseCommand = new Command<Course>(async (course) => await DeleteCourseAsync(course));
        CourseSelectedCommand = new Command<Course>(async (course) => await OnCourseSelected(course));
    }

    public ObservableCollection<Course> Courses
    {
        get => _courses;
        set => SetProperty(ref _courses, value);
    }

    public ObservableCollection<Course> FilteredCourses
    {
        get => _filteredCourses;
        set => SetProperty(ref _filteredCourses, value);
    }

    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (SetProperty(ref _searchQuery, value))
                ApplySearch();
        }
    }

    public ICommand LoadCoursesCommand { get; }
    public ICommand AddCourseCommand { get; }
    public ICommand DeleteCourseCommand { get; }
    public ICommand CourseSelectedCommand { get; }

    public async Task InitializeAsync()
    {
        await LoadCoursesAsync();
    }

    public async Task LoadCoursesAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;

        try
        {
            // Очищаємо попередні дані
            Courses.Clear();
            FilteredCourses.Clear();

            var courses = await _courseService.GetAllCoursesAsync();
            foreach (var course in courses)
            {
                Courses.Add(course);
                FilteredCourses.Add(course);
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

    private void ApplySearch()
    {
        FilteredCourses.Clear();

        var filtered = string.IsNullOrWhiteSpace(SearchQuery)
            ? Courses
            : Courses.Where(c =>
                c.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                (c.Description?.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ?? false));

        foreach (var course in filtered)
            FilteredCourses.Add(course);
    }

    private async Task AddCourseAsync()
    {
        string courseName = await Application.Current.MainPage.DisplayPromptAsync(
            "Новий предмет",
            "Введіть назву предмету:",
            "Створити",
            "Скасувати");

        if (string.IsNullOrWhiteSpace(courseName))
            return;

        var colors = new[]
        {
            "#F05454",  // червоний
            "#F4845F",  // помаранчевий
            "#F4A261",  // персиковий
            "#F9C74F",  // жовтий
            "#90BE6D",  // світло-зелений
            "#52B788",  // середній зелений
            "#3AAFA9",  // бірюзовий
            "#5B8CDB",  // синьо-фіолетовий
            "#7B6CF6",  // фіолетовий
            "#C77DFF",  // світло-фіолетовий
            "#F472B6",  // рожевий
            "#E05C97",  // малиновий
        };

        var random = new Random();

        var course = new Course
        {
            Name = courseName,
            Description = "",
            Color = colors[random.Next(colors.Length)],
            Progress = 0
        };

        await _courseService.SaveCourseAsync(course);
        await LoadCoursesAsync();
    }

    private async Task DeleteCourseAsync(Course course)
    {
        if (course == null)
            return;

        bool confirm = await Application.Current.MainPage.DisplayAlert(
            "Видалити предмет?",
            $"Видалити '{course.Name}' та всі його завдання?",
            "Видалити",
            "Скасувати");

        if (!confirm)
            return;

        await _courseService.DeleteCourseAsync(course.Id);
        await LoadCoursesAsync();
    }

    private async Task OnCourseSelected(Course course)
    {
        if (course == null)
            return;

        await Shell.Current.GoToAsync($"TasksPage?courseId={course.Id}");
    }
}