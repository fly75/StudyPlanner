namespace MauiApp16.Services;

public class SecureStorageService : ISecureStorageService
{
    public async Task SetAsync(string key, string value)
    {
        await SecureStorage.SetAsync(key, value);
    }

    public async Task<string> GetAsync(string key)
    {
        try
        {
            return await SecureStorage.GetAsync(key);
        }
        catch
        {
            return null;
        }
    }

    public async Task RemoveAsync(string key)
    {
        SecureStorage.Remove(key);
        await Task.CompletedTask;
    }
}