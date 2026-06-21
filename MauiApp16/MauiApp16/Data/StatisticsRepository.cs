using MauiApp16.Models;

namespace MauiApp16.Data;

public class StatisticsRepository : IRepository<Statistics>
{
    private readonly DatabaseContext _context;

    public StatisticsRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<List<Statistics>> GetAllAsync()
    {
        return await _context.GetAllAsync<Statistics>();
    }

    public async Task<List<Statistics>> GetByUserIdAsync(int userId, DateTime from, DateTime to)
    {
        return await _context.QueryAsync<Statistics>(
            "SELECT * FROM Statistics WHERE UserId = ? AND Date >= ? AND Date <= ?",
            userId, from, to);
    }

    public async Task<Statistics> GetByIdAsync(int id)
    {
        return await _context.GetAsync<Statistics>(id);
    }

    public async Task<int> SaveAsync(Statistics item)
    {
        return await _context.SaveAsync(item);
    }

    public async Task<int> DeleteAsync(Statistics item)
    {
        return await _context.DeleteAsync(item);
    }
}