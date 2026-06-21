using MauiApp16.Models;
using SQLite;

namespace MauiApp16.Data;

public class DatabaseContext
{
    private SQLiteAsyncConnection _database;
    private readonly string _dbPath;

    public DatabaseContext(string dbPath) { _dbPath = dbPath; }

    private async Task InitAsync()
    {
        if (_database != null)
            return;

        _database = new SQLiteAsyncConnection(_dbPath);

        await _database.CreateTableAsync<User>();
        await _database.CreateTableAsync<Course>();
        await _database.CreateTableAsync<TaskModel>();
        await _database.CreateTableAsync<Statistics>();
        await _database.CreateTableAsync<AppNotification>();
    }

    public async Task<List<T>> GetAllAsync<T>() where T : new()
    {
        await InitAsync();
        return await _database.Table<T>().ToListAsync();
    }

    public async Task<T> GetAsync<T>(int id) where T : class, IEntity, new()
    {
        await InitAsync();
        return await _database.Table<T>()
            .Where(i => i.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<int> SaveAsync<T>(T item) where T : class, IEntity, new()
    {
        await InitAsync();
        var id = (item as dynamic).Id;
        if (id != 0)
            return await _database.UpdateAsync(item);
        else
            return await _database.InsertAsync(item);
    }

    public async Task<int> DeleteAsync<T>(T item) where T : new()
    {
        await InitAsync();
        return await _database.DeleteAsync(item);
    }

    public async Task<List<T>> QueryAsync<T>(string query, params object[] args) where T : new()
    {
        await InitAsync();
        return await _database.QueryAsync<T>(query, args);
    }

    public async Task<int> ExecuteAsync(string query, params object[] args)
    {
        await InitAsync();
        return await _database.ExecuteAsync(query, args);
    }
}