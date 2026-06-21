namespace MauiApp16.Services;

public interface ITestDataService
{
    Task SeedTestDataAsync();
    Task<bool> HasTestDataAsync();
}