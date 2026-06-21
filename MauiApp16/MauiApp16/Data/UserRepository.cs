using MauiApp16.Models;

namespace MauiApp16.Data;

public class UserRepository : IRepository<User>
{
    private readonly DatabaseContext _context;

    public UserRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _context.GetAllAsync<User>();
    }

    public async Task<User> GetByIdAsync(int id)
    {
        return await _context.GetAsync<User>(id);
    }

    public async Task<User> GetByEmailAsync(string email)
    {
        var users = await _context.QueryAsync<User>(
            "SELECT * FROM User WHERE Email = ?", email);
        return users.FirstOrDefault();
    }

    public async Task<int> SaveAsync(User item)
    {
        return await _context.SaveAsync(item);
    }

    public async Task<int> DeleteAsync(User item)
    {
        return await _context.DeleteAsync(item);
    }
}