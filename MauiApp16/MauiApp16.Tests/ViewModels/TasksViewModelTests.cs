using Xunit;
using Moq;
using MauiApp16.ViewModels;
using MauiApp16.Services;
using MauiApp16.Models;

namespace MauiApp16.Tests.ViewModels;

public class TasksViewModelTests
{
    private readonly Mock<ITaskService>   _taskServiceMock;
    private readonly Mock<ICourseService> _courseServiceMock;
    private readonly Mock<IExportService> _exportServiceMock;
    private readonly TasksViewModel _viewModel;

    public TasksViewModelTests()
    {
        _taskServiceMock   = new Mock<ITaskService>();
        _courseServiceMock = new Mock<ICourseService>();
        _exportServiceMock = new Mock<IExportService>();
        _viewModel = new TasksViewModel(
            _taskServiceMock.Object,
            _courseServiceMock.Object,
            _exportServiceMock.Object);
    }

    // ──────────────────────────────────────────────
    // Початковий стан
    // ──────────────────────────────────────────────

    [Fact]
    public void Constructor_InitialState_TasksCollectionNotNull()
    {
        Assert.NotNull(_viewModel.Tasks);
    }

    [Fact]
    public void Constructor_InitialState_TasksCollectionEmpty()
    {
        Assert.Empty(_viewModel.Tasks);
    }

    [Fact]
    public void Constructor_InitialState_IsNotBusy()
    {
        Assert.False(_viewModel.IsBusy);
    }

    [Fact]
    public void Constructor_InitialState_CommandsNotNull()
    {
        Assert.NotNull(_viewModel.LoadTasksCommand);
        Assert.NotNull(_viewModel.AddTaskCommand);
        Assert.NotNull(_viewModel.CompleteTaskCommand);
    }

    [Fact]
    public void Constructor_InitialState_FilteredTasksNotNull()
    {
        Assert.NotNull(_viewModel.FilteredTasks);
    }

    // ──────────────────────────────────────────────
    // LoadTasksAsync — напряму через публічний метод
    // ──────────────────────────────────────────────

    [Fact]
    public async Task LoadTasksAsync_ServiceReturnsTasks_CollectionPopulated()
    {
        var tasks = new List<TaskModel>
        {
            new TaskModel { Id = 1, Title = "Лабораторна №1", Deadline = DateTime.Now.AddDays(1) },
            new TaskModel { Id = 2, Title = "Курсова робота",  Deadline = DateTime.Now.AddDays(2) }
        };
        _taskServiceMock
            .Setup(s => s.GetTasksByCourseAsync(It.IsAny<int>()))
            .ReturnsAsync(tasks);
        _taskServiceMock
            .Setup(s => s.SearchTasksAsync(
                It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<MauiApp16.Models.TaskStatus?>(),
                It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(tasks);

        await _viewModel.LoadTasksAsync();

        Assert.Equal(2, _viewModel.Tasks.Count);
    }

    [Fact]
    public async Task LoadTasksAsync_ServiceReturnsEmpty_CollectionEmpty()
    {
        _taskServiceMock
            .Setup(s => s.GetTasksByCourseAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<TaskModel>());
        _taskServiceMock
            .Setup(s => s.SearchTasksAsync(
                It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<MauiApp16.Models.TaskStatus?>(),
                It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(new List<TaskModel>());

        await _viewModel.LoadTasksAsync();

        Assert.Empty(_viewModel.Tasks);
    }

    // ──────────────────────────────────────────────
    // SearchQuery — прив'язка даних
    // ──────────────────────────────────────────────

    [Fact]
    public void SearchQuery_SetValue_PropertyChangedRaised()
    {
        bool raised = false;
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_viewModel.SearchQuery))
                raised = true;
        };

        _viewModel.SearchQuery = "математика";

        Assert.True(raised);
        Assert.Equal("математика", _viewModel.SearchQuery);
    }

    // ──────────────────────────────────────────────
    // SelectedStatusIndex
    // ──────────────────────────────────────────────

    [Fact]
    public void SelectedStatusIndex_DefaultValue_IsZero()
    {
        Assert.Equal(0, _viewModel.SelectedStatusIndex);
    }

    [Fact]
    public void SelectedStatusIndex_SetValue_PropertyChangedRaised()
    {
        bool raised = false;
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_viewModel.SelectedStatusIndex))
                raised = true;
        };

        _viewModel.SelectedStatusIndex = 1;

        Assert.True(raised);
    }
}
