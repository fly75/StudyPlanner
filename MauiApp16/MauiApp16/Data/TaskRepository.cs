using MauiApp16.Models;

namespace MauiApp16.Data;

public class TaskRepository : IRepository<TaskModel>
{
    private readonly DatabaseContext _context;

    public TaskRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<List<TaskModel>> GetAllAsync()
    {
        return await _context.GetAllAsync<TaskModel>();
    }

    public async Task<List<TaskModel>> GetByCourseIdAsync(int courseId)
    {
        return await _context.QueryAsync<TaskModel>(
            "SELECT * FROM TaskModel WHERE CourseId = ?", courseId);
    }

    public async Task<List<TaskModel>> GetByStatusAsync(Models.TaskStatus status)
    {
        return await _context.QueryAsync<TaskModel>(
            "SELECT * FROM TaskModel WHERE Status = ?", (int)status);
    }

    public async Task<List<TaskModel>> GetUpcomingAsync(DateTime from, DateTime to)
    {
        return await _context.QueryAsync<TaskModel>(
            "SELECT * FROM TaskModel WHERE Deadline >= ? AND Deadline <= ? AND Status != ?",
            from, to, (int)Models.TaskStatus.Completed);
    }

    public async Task<TaskModel> GetByIdAsync(int id)
    {
        return await _context.GetAsync<TaskModel>(id);
    }

    public async Task<int> SaveAsync(TaskModel item)
    {
        return await _context.SaveAsync(item);
    }

    public async Task<int> DeleteAsync(TaskModel item)
    {
        return await _context.DeleteAsync(item);
    }

    public async Task<int> DeleteByCourseIdAsync(int courseId)
    {
        return await _context.ExecuteAsync(
            "DELETE FROM TaskModel WHERE CourseId = ?", courseId);
    }
}