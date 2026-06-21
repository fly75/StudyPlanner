using MauiApp16.Data;
using MauiApp16.Models;

namespace MauiApp16.Services;

public class CourseService : ICourseService
{
    private readonly CourseRepository _courseRepository;
    private readonly TaskRepository _taskRepository;
    private readonly IAuthService _authService;

    public CourseService(CourseRepository courseRepository, TaskRepository taskRepository, IAuthService authService)
    {
        _courseRepository = courseRepository;
        _taskRepository = taskRepository;
        _authService = authService;
    }

    public async Task<List<Course>> GetAllCoursesAsync()
    {
        var user = await _authService.GetCurrentUserAsync();
        if (user == null)
            return new List<Course>();

        var courses = await _courseRepository.GetByUserIdAsync(user.Id);

        foreach (var course in courses)
        {
            await UpdateCourseProgressAsync(course.Id);
        }

        return courses;
    }

    public async Task<Course> GetCourseByIdAsync(int id)
    {
        return await _courseRepository.GetByIdAsync(id);
    }

    public async Task<bool> SaveCourseAsync(Course course)
    {
        var user = await _authService.GetCurrentUserAsync();
        if (user == null)
            return false;

        if (course.Id == 0)
        {
            course.UserId = user.Id;
            course.CreatedAt = DateTime.Now;
        }

        await _courseRepository.SaveAsync(course);
        return true;
    }

    public async Task<bool> DeleteCourseAsync(int courseId)
    {
        await _taskRepository.DeleteByCourseIdAsync(courseId);

        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course != null)
        {
            await _courseRepository.DeleteAsync(course);
            return true;
        }

        return false;
    }

    public async Task UpdateCourseProgressAsync(int courseId)
    {
        var tasks = await _taskRepository.GetByCourseIdAsync(courseId);
        if (tasks.Count == 0)
            return;

        var completedTasks = tasks.Count(t => t.Status == Models.TaskStatus.Completed);
        var progress = (double)completedTasks / tasks.Count * 100;

        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course != null)
        {
            course.Progress = Math.Round(progress, 2);
            await _courseRepository.SaveAsync(course);
        }
    }
}