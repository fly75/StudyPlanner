using Xunit;
using Moq;
using MauiApp16.ViewModels;
using MauiApp16.Services;
using MauiApp16.Models;

namespace MauiApp16.Tests.ViewModels;

public class CoursesViewModelTests
{
    private readonly Mock<ICourseService> _courseServiceMock;
    private readonly CoursesViewModel _viewModel;

    public CoursesViewModelTests()
    {
        _courseServiceMock = new Mock<ICourseService>();
        _viewModel = new CoursesViewModel(_courseServiceMock.Object);
    }

    // ──────────────────────────────────────────────
    // Початковий стан
    // ──────────────────────────────────────────────

    [Fact]
    public void Constructor_InitialState_CoursesCollectionNotNull()
    {
        Assert.NotNull(_viewModel.Courses);
    }

    [Fact]
    public void Constructor_InitialState_CoursesCollectionEmpty()
    {
        Assert.Empty(_viewModel.Courses);
    }

    [Fact]
    public void Constructor_InitialState_IsNotBusy()
    {
        Assert.False(_viewModel.IsBusy);
    }

    [Fact]
    public void Constructor_InitialState_CommandsNotNull()
    {
        Assert.NotNull(_viewModel.LoadCoursesCommand);
        Assert.NotNull(_viewModel.AddCourseCommand);
        Assert.NotNull(_viewModel.DeleteCourseCommand);
    }

    [Fact]
    public void Constructor_InitialState_FilteredCoursesNotNull()
    {
        Assert.NotNull(_viewModel.FilteredCourses);
    }

    // ──────────────────────────────────────────────
    // LoadCoursesAsync — напряму через публічний метод
    // ──────────────────────────────────────────────

    [Fact]
    public async Task LoadCoursesAsync_ServiceReturnsCourses_CollectionPopulated()
    {
        var courses = new List<Course>
        {
            new Course { Id = 1, Name = "Математичний аналіз", Color = "#FF5733" },
            new Course { Id = 2, Name = "Програмування",       Color = "#3498DB" },
            new Course { Id = 3, Name = "Фізика",              Color = "#2ECC71" }
        };
        _courseServiceMock
            .Setup(s => s.GetAllCoursesAsync())
            .ReturnsAsync(courses);

        await _viewModel.LoadCoursesAsync();

        Assert.Equal(3, _viewModel.Courses.Count);
    }

    [Fact]
    public async Task LoadCoursesAsync_ServiceReturnsEmpty_CollectionEmpty()
    {
        _courseServiceMock
            .Setup(s => s.GetAllCoursesAsync())
            .ReturnsAsync(new List<Course>());

        await _viewModel.LoadCoursesAsync();

        Assert.Empty(_viewModel.Courses);
    }

    [Fact]
    public async Task LoadCoursesAsync_PopulatesFilteredCoursesToo()
    {
        var courses = new List<Course>
        {
            new Course { Id = 1, Name = "Хімія" },
            new Course { Id = 2, Name = "Біологія" }
        };
        _courseServiceMock
            .Setup(s => s.GetAllCoursesAsync())
            .ReturnsAsync(courses);

        await _viewModel.LoadCoursesAsync();

        Assert.Equal(2, _viewModel.FilteredCourses.Count);
    }

    // ──────────────────────────────────────────────
    // SearchQuery — фільтрація
    // ──────────────────────────────────────────────

    [Fact]
    public async Task SearchQuery_SetValue_FilteredCoursesUpdated()
    {
        var courses = new List<Course>
        {
            new Course { Id = 1, Name = "Математика" },
            new Course { Id = 2, Name = "Фізика"     }
        };
        _courseServiceMock
            .Setup(s => s.GetAllCoursesAsync())
            .ReturnsAsync(courses);

        await _viewModel.LoadCoursesAsync();
        _viewModel.SearchQuery = "Мат";

        Assert.Single(_viewModel.FilteredCourses);
        Assert.Equal("Математика", _viewModel.FilteredCourses[0].Name);
    }

    [Fact]
    public void SearchQuery_SetValue_PropertyChangedRaised()
    {
        bool raised = false;
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_viewModel.SearchQuery))
                raised = true;
        };

        _viewModel.SearchQuery = "фізика";

        Assert.True(raised);
    }
}
