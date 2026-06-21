using Xunit;
using Moq;
using MauiApp16.Services;
using MauiApp16.Models;

namespace MauiApp16.Tests.Services;

// Тестуємо IStatisticsService через Mock — без реальної БД
public class StatisticsServiceTests
{
    private readonly Mock<IStatisticsService> _statisticsServiceMock;

    public StatisticsServiceTests()
    {
        _statisticsServiceMock = new Mock<IStatisticsService>();
    }

    // ──────────────────────────────────────────────
    // GetCompletionPercentageAsync
    // ──────────────────────────────────────────────

    [Fact]
    public async Task GetCompletionPercentageAsync_NoTasks_ReturnsZero()
    {
        _statisticsServiceMock
            .Setup(s => s.GetCompletionPercentageAsync())
            .ReturnsAsync(0.0);

        var result = await _statisticsServiceMock.Object.GetCompletionPercentageAsync();

        Assert.Equal(0.0, result);
    }

    [Fact]
    public async Task GetCompletionPercentageAsync_AllCompleted_Returns100()
    {
        _statisticsServiceMock
            .Setup(s => s.GetCompletionPercentageAsync())
            .ReturnsAsync(100.0);

        var result = await _statisticsServiceMock.Object.GetCompletionPercentageAsync();

        Assert.Equal(100.0, result);
    }

    [Fact]
    public async Task GetCompletionPercentageAsync_HalfCompleted_Returns50()
    {
        _statisticsServiceMock
            .Setup(s => s.GetCompletionPercentageAsync())
            .ReturnsAsync(50.0);

        var result = await _statisticsServiceMock.Object.GetCompletionPercentageAsync();

        Assert.Equal(50.0, result);
    }

    // ──────────────────────────────────────────────
    // GetCurrentStreakAsync
    // ──────────────────────────────────────────────

    [Fact]
    public async Task GetCurrentStreakAsync_NoActivity_ReturnsZero()
    {
        _statisticsServiceMock
            .Setup(s => s.GetCurrentStreakAsync())
            .ReturnsAsync(0);

        var result = await _statisticsServiceMock.Object.GetCurrentStreakAsync();

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task GetCurrentStreakAsync_ActiveStreak_ReturnsPositive()
    {
        _statisticsServiceMock
            .Setup(s => s.GetCurrentStreakAsync())
            .ReturnsAsync(5);

        var result = await _statisticsServiceMock.Object.GetCurrentStreakAsync();

        Assert.Equal(5, result);
        Assert.True(result > 0);
    }

    // ──────────────────────────────────────────────
    // GetCompletedTasksChartDataAsync
    // ──────────────────────────────────────────────

    [Fact]
    public async Task GetCompletedTasksChartDataAsync_ReturnsData()
    {
        var data = new Dictionary<string, int>
        {
            { "Пн", 2 }, { "Вт", 3 }, { "Ср", 1 }
        };
        _statisticsServiceMock
            .Setup(s => s.GetCompletedTasksChartDataAsync(7))
            .ReturnsAsync(data);

        var result = await _statisticsServiceMock.Object.GetCompletedTasksChartDataAsync(7);

        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetCompletedTasksChartDataAsync_NoData_ReturnsEmpty()
    {
        _statisticsServiceMock
            .Setup(s => s.GetCompletedTasksChartDataAsync(It.IsAny<int>()))
            .ReturnsAsync(new Dictionary<string, int>());

        var result = await _statisticsServiceMock.Object.GetCompletedTasksChartDataAsync(7);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    // ──────────────────────────────────────────────
    // GetBestStreakAsync
    // ──────────────────────────────────────────────

    [Fact]
    public async Task GetBestStreakAsync_ReturnsMaxStreak()
    {
        _statisticsServiceMock
            .Setup(s => s.GetBestStreakAsync())
            .ReturnsAsync(14);

        var result = await _statisticsServiceMock.Object.GetBestStreakAsync();

        Assert.Equal(14, result);
    }
}
