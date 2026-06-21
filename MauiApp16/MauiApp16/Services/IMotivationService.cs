namespace MauiApp16.Services;

public interface IMotivationService
{
    Task<string> GetMotivationalMessageAsync();
}