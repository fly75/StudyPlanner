using MauiApp16.Models;

namespace MauiApp16.Data;

public class CourseRepository : IRepository<Course>
{
    private readonly DatabaseContext _context;

    public CourseRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<List<Course>> GetAllAsync()
    {
        return await _context.GetAllAsync<Course>();
    }

    public async Task<List<Course>> GetByUserIdAsync(int userId)
    {
        return await _context.QueryAsync<Course>(
            "SELECT * FROM Course WHERE UserId = ?", userId);
    }

    public async Task<Course> GetByIdAsync(int id)
    {
        return await _context.GetAsync<Course>(id);
    }

    public async Task<int> SaveAsync(Course item)
    {
        return await _context.SaveAsync(item);
    }

    public async Task<int> DeleteAsync(Course item)
    {
        return await _context.DeleteAsync(item);
    }
}