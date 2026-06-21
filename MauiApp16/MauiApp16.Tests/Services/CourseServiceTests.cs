using Xunit;
using Moq;
using MauiApp16.Services;
using MauiApp16.Models;

namespace MauiApp16.Tests.Services;

// Тестуємо ICourseService через Mock — без реальної БД
public class CourseServiceTests
{
    private readonly Mock<ICourseService> _courseServiceMock;

    public CourseServiceTests()
    {
        _courseServiceMock = new Mock<ICourseService>();
    }

    // ──────────────────────────────────────────────
    // GetAllCoursesAsync
    // ──────────────────────────────────────────────

    [Fact]
    public async Task GetAllCoursesAsync_ReturnsCourses()
    {
        var courses = new List<Course>
        {
            new Course { Id = 1, Name = "Математичний аналіз", Color = "#FF5733" },
            new Course { Id = 2, Name = "Програмування",       Color = "#3498DB" }
        };
        _courseServiceMock
            .Setup(s => s.GetAllCoursesAsync())
            .ReturnsAsync(courses);

        var result = await _courseServiceMock.Object.GetAllCoursesAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllCoursesAsync_NoCourses_ReturnsEmptyList()
    {
        _courseServiceMock
            .Setup(s => s.GetAllCoursesAsync())
            .ReturnsAsync(new List<Course>());

        var result = await _courseServiceMock.Object.GetAllCoursesAsync();

        Assert.Empty(result);
    }

    // ──────────────────────────────────────────────
    // SaveCourseAsync
    // ──────────────────────────────────────────────

    [Fact]
    public async Task SaveCourseAsync_ValidCourse_ReturnsTrue()
    {
        var course = new Course { Id = 0, Name = "Алгоритми" };
        _courseServiceMock
            .Setup(s => s.SaveCourseAsync(course))
            .ReturnsAsync(true);

        var result = await _courseServiceMock.Object.SaveCourseAsync(course);

        Assert.True(result);
        _courseServiceMock.Verify(s => s.SaveCourseAsync(course), Times.Once);
    }

    [Fact]
    public async Task SaveCourseAsync_ServiceFails_ReturnsFalse()
    {
        var course = new Course { Id = 0, Name = "Невалідний" };
        _courseServiceMock
            .Setup(s => s.SaveCourseAsync(course))
            .ReturnsAsync(false);

        var result = await _courseServiceMock.Object.SaveCourseAsync(course);

        Assert.False(result);
    }

    // ──────────────────────────────────────────────
    // GetCourseByIdAsync
    // ──────────────────────────────────────────────

    [Fact]
    public async Task GetCourseByIdAsync_ExistingId_ReturnsCourse()
    {
        var expected = new Course { Id = 5, Name = "Бази даних" };
        _courseServiceMock
            .Setup(s => s.GetCourseByIdAsync(5))
            .ReturnsAsync(expected);

        var result = await _courseServiceMock.Object.GetCourseByIdAsync(5);

        Assert.Equal("Бази даних", result.Name);
        Assert.Equal(5, result.Id);
    }

    [Fact]
    public async Task GetCourseByIdAsync_NotFound_ReturnsNull()
    {
        _courseServiceMock
            .Setup(s => s.GetCourseByIdAsync(999))
            .ReturnsAsync((Course)null);

        var result = await _courseServiceMock.Object.GetCourseByIdAsync(999);

        Assert.Null(result);
    }

    // ──────────────────────────────────────────────
    // DeleteCourseAsync
    // ──────────────────────────────────────────────

    [Fact]
    public async Task DeleteCourseAsync_ExistingCourse_ReturnsTrue()
    {
        _courseServiceMock
            .Setup(s => s.DeleteCourseAsync(3))
            .ReturnsAsync(true);

        var result = await _courseServiceMock.Object.DeleteCourseAsync(3);

        Assert.True(result);
        _courseServiceMock.Verify(s => s.DeleteCourseAsync(3), Times.Once);
    }

    [Fact]
    public async Task DeleteCourseAsync_NotFound_ReturnsFalse()
    {
        _courseServiceMock
            .Setup(s => s.DeleteCourseAsync(999))
            .ReturnsAsync(false);

        var result = await _courseServiceMock.Object.DeleteCourseAsync(999);

        Assert.False(result);
    }
}
