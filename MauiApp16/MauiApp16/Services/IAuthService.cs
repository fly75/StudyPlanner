using MauiApp16.Models;

namespace MauiApp16.Services;

public interface IAuthService
{
    Task<bool> LoginAsync(string email, string password);
    Task<bool> RegisterAsync(string email, string password);
    Task LogoutAsync();
    Task<User> GetCurrentUserAsync();
    Task<bool> DeleteAccountAsync();
    bool IsAuthenticated { get; }

    Task<bool> UpdateProfileAsync(
        string firstName, string lastName,
        DateTime? birthDate, string phoneNumber,
        string country,
        string avatarPath = null);

    Task<bool> ChangePasswordAsync(string currentPassword, string newPassword);
}