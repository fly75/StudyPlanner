using MauiApp16.Models;

namespace MauiApp16.Services;

public interface ICourseService
{
    Task<List<Course>> GetAllCoursesAsync();
    Task<Course> GetCourseByIdAsync(int id);
    Task<bool> SaveCourseAsync(Course course);
    Task<bool> DeleteCourseAsync(int courseId);
    Task UpdateCourseProgressAsync(int courseId);
}