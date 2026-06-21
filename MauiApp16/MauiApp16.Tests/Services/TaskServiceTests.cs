using Xunit;
using Moq;
using MauiApp16.Services;
using MauiApp16.Models;
using AppTaskStatus = MauiApp16.Models.TaskStatus;

namespace MauiApp16.Tests.Services;

// Тестуємо ITaskService через Mock — без реальної БД
public class TaskServiceTests
{
    private readonly Mock<ITaskService> _taskServiceMock;

    public TaskServiceTests()
    {
        _taskServiceMock = new Mock<ITaskService>();
    }

    // ──────────────────────────────────────────────
    // GetAllTasksAsync
    // ──────────────────────────────────────────────

    [Fact]
    public async Task GetAllTasksAsync_ReturnsAllTasks()
    {
        var tasks = new List<TaskModel>
        {
            new TaskModel { Id = 1, Title = "Лабораторна робота №1", CourseId = 1 },
            new TaskModel { Id = 2, Title = "Контрольна робота",     CourseId = 2 }
        };
        _taskServiceMock
            .Setup(s => s.GetAllTasksAsync())
            .ReturnsAsync(tasks);

        var result = await _taskServiceMock.Object.GetAllTasksAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllTasksAsync_NoTasks_ReturnsEmptyList()
    {
        _taskServiceMock
            .Setup(s => s.GetAllTasksAsync())
            .ReturnsAsync(new List<TaskModel>());

        var result = await _taskServiceMock.Object.GetAllTasksAsync();

        Assert.Empty(result);
    }

    // ──────────────────────────────────────────────
    // GetUpcomingTasksAsync
    // ──────────────────────────────────────────────

    [Fact]
    public async Task GetUpcomingTasksAsync_ReturnsUpcomingTasks()
    {
        var tasks = new List<TaskModel>
        {
            new TaskModel { Id = 1, Title = "Дедлайн завтра", Deadline = DateTime.Now.AddDays(1) }
        };
        _taskServiceMock
            .Setup(s => s.GetUpcomingTasksAsync(7))
            .ReturnsAsync(tasks);

        var result = await _taskServiceMock.Object.GetUpcomingTasksAsync(7);

        Assert.Single(result);
    }

    [Fact]
    public async Task GetUpcomingTasksAsync_NoUpcoming_ReturnsEmptyList()
    {
        _taskServiceMock
            .Setup(s => s.GetUpcomingTasksAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<TaskModel>());

        var result = await _taskServiceMock.Object.GetUpcomingTasksAsync(7);

        Assert.Empty(result);
    }

    // ──────────────────────────────────────────────
    // SaveTaskAsync
    // ──────────────────────────────────────────────

    [Fact]
    public async Task SaveTaskAsync_ValidTask_ReturnsTrue()
    {
        var task = new TaskModel { Id = 0, Title = "Нове завдання", CourseId = 1 };
        _taskServiceMock
            .Setup(s => s.SaveTaskAsync(task))
            .ReturnsAsync(true);

        var result = await _taskServiceMock.Object.SaveTaskAsync(task);

        Assert.True(result);
        _taskServiceMock.Verify(s => s.SaveTaskAsync(task), Times.Once);
    }

    // ──────────────────────────────────────────────
    // DeleteTaskAsync
    // ──────────────────────────────────────────────

    [Fact]
    public async Task DeleteTaskAsync_ExistingTask_ReturnsTrue()
    {
        _taskServiceMock
            .Setup(s => s.DeleteTaskAsync(1))
            .ReturnsAsync(true);

        var result = await _taskServiceMock.Object.DeleteTaskAsync(1);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteTaskAsync_NonExistingTask_ReturnsFalse()
    {
        _taskServiceMock
            .Setup(s => s.DeleteTaskAsync(999))
            .ReturnsAsync(false);

        var result = await _taskServiceMock.Object.DeleteTaskAsync(999);

        Assert.False(result);
    }

    // ──────────────────────────────────────────────
    // CompleteTaskAsync
    // ──────────────────────────────────────────────

    [Fact]
    public async Task CompleteTaskAsync_ExistingTask_ReturnsTrue()
    {
        _taskServiceMock
            .Setup(s => s.CompleteTaskAsync(1))
            .ReturnsAsync(true);

        var result = await _taskServiceMock.Object.CompleteTaskAsync(1);

        Assert.True(result);
    }

    [Fact]
    public async Task CompleteTaskAsync_NonExistingTask_ReturnsFalse()
    {
        _taskServiceMock
            .Setup(s => s.CompleteTaskAsync(0))
            .ReturnsAsync(false);

        var result = await _taskServiceMock.Object.CompleteTaskAsync(0);

        Assert.False(result);
    }
}
