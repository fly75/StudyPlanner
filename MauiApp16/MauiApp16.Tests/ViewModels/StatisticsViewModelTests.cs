using Xunit;
using Moq;
using MauiApp16.ViewModels;
using MauiApp16.Services;
using MauiApp16.Models;

namespace MauiApp16.Tests.ViewModels;

public class StatisticsViewModelTests
{
    private readonly Mock<IStatisticsService> _statisticsServiceMock;
    private readonly Mock<ITaskService> _taskServiceMock;
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<ICourseService> _courseServiceMock;
    private readonly StatisticsViewModel _viewModel;

    public StatisticsViewModelTests()
    {
        _statisticsServiceMock = new Mock<IStatisticsService>();
        _taskServiceMock = new Mock<ITaskService>();
        _authServiceMock = new Mock<IAuthService>();
        _courseServiceMock = new Mock<ICourseService>();

        _viewModel = new StatisticsViewModel(
            _statisticsServiceMock.Object,
            _taskServiceMock.Object,
            _authServiceMock.Object,
            _courseServiceMock.Object);
    }

    // ──────────────────────────────────────────────────────────
    // Хелпер: налаштовує ВСІ моки, які викликає LoadDataAsync()
    // ──────────────────────────────────────────────────────────
    private void SetupAllMocks(
        double completionPercentage = 0.0,
        int currentStreak = 0,
        int bestStreak = 0,
        double avgHours = 0.0)
    {
        _statisticsServiceMock
            .Setup(s => s.GetCompletionPercentageAsync())
            .ReturnsAsync(completionPercentage);

        _statisticsServiceMock
            .Setup(s => s.GetCurrentStreakAsync())
            .ReturnsAsync(currentStreak);

        _statisticsServiceMock
            .Setup(s => s.GetBestStreakAsync())
            .ReturnsAsync(bestStreak);

        _statisticsServiceMock
            .Setup(s => s.GetCompletedTasksChartDataAsync(It.IsAny<int>()))
            .ReturnsAsync(new Dictionary<string, int>());

        _statisticsServiceMock
            .Setup(s => s.GetAverageCompletionTimeHoursAsync())
            .ReturnsAsync(avgHours);

        _statisticsServiceMock
            .Setup(s => s.GetTopCourseAsync())
            .ReturnsAsync(("", 0));

        _statisticsServiceMock
            .Setup(s => s.GetProductivityByWeekDayAsync())
            .ReturnsAsync(new Dictionary<DayOfWeek, int>
            {
                { DayOfWeek.Monday,    0 },
                { DayOfWeek.Tuesday,   0 },
                { DayOfWeek.Wednesday, 0 },
                { DayOfWeek.Thursday,  0 },
                { DayOfWeek.Friday,    0 },
                { DayOfWeek.Saturday,  0 },
                { DayOfWeek.Sunday,    0 },
            });

        _statisticsServiceMock
            .Setup(s => s.GetTasksByPriorityAsync())
            .ReturnsAsync(new Dictionary<TaskPriority, int>());

        // null → блок завантаження курсів/завдань пропускається
        _authServiceMock
            .Setup(s => s.GetCurrentUserAsync())
            .ReturnsAsync((User?)null);

        _courseServiceMock
            .Setup(s => s.GetAllCoursesAsync())
            .ReturnsAsync(new List<Course>());

        _taskServiceMock
            .Setup(s => s.GetAllTasksAsync())
            .ReturnsAsync(new List<TaskModel>());
    }

    // ──────────────────────────────────────────────
    // Початковий стан
    // ──────────────────────────────────────────────

    [Fact]
    public void Constructor_InitialState_CompletionPercentageIsZero()
    {
        Assert.Equal(0.0, _viewModel.CompletionPercentage);
    }

    [Fact]
    public void Constructor_InitialState_IsNotBusy()
    {
        Assert.False(_viewModel.IsBusy);
    }

    [Fact]
    public void Constructor_InitialState_LoadCommandNotNull()
    {
        Assert.NotNull(_viewModel.LoadDataCommand);
    }

    // ──────────────────────────────────────────────
    // LoadDataCommand — завантаження статистики
    // ──────────────────────────────────────────────

    [Fact]
    public async Task LoadDataCommand_ServiceReturns75_SetsCompletionPercentage()
    {
        SetupAllMocks(completionPercentage: 75.0, currentStreak: 5);

        _viewModel.LoadDataCommand.Execute(null);
        await Task.Delay(500);

        Assert.Equal(75.0, _viewModel.CompletionPercentage);
    }

    [Fact]
    public async Task LoadDataCommand_ServiceReturnsStreak5_SetsCurrentStreak()
    {
        SetupAllMocks(completionPercentage: 0.0, currentStreak: 5);

        _viewModel.LoadDataCommand.Execute(null);
        await Task.Delay(500);

        Assert.Equal(5, _viewModel.CurrentStreak);
    }

    // ──────────────────────────────────────────────
    // CompletionPercentage — граничні значення
    // ──────────────────────────────────────────────

    [Theory]
    [InlineData(0.0)]
    [InlineData(50.0)]
    [InlineData(100.0)]
    public void CompletionPercentage_BoundaryValues_SetCorrectly(double value)
    {
        _viewModel.CompletionPercentage = value;
        Assert.Equal(value, _viewModel.CompletionPercentage);
    }
}