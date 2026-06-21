using System.Security.Cryptography;
using System.Text;
using MauiApp16.Data;
using MauiApp16.Models;

namespace MauiApp16.Services;

public class AuthService : IAuthService
{
    private readonly UserRepository _userRepository;
    private readonly CourseRepository _courseRepository;
    private readonly TaskRepository _taskRepository;
    private readonly StatisticsRepository _statisticsRepository;
    private readonly ISecureStorageService _secureStorage;
    private User _currentUser;

    public AuthService(
        UserRepository userRepository,
        CourseRepository courseRepository,
        TaskRepository taskRepository,
        StatisticsRepository statisticsRepository,
        ISecureStorageService secureStorage)
    {
        _userRepository = userRepository;
        _courseRepository = courseRepository;
        _taskRepository = taskRepository;
        _statisticsRepository = statisticsRepository;
        _secureStorage = secureStorage;
    }

    public bool IsAuthenticated => _currentUser != null;

    public async Task<bool> LoginAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
            return false;

        var passwordHash = HashPassword(password);
        if (user.PasswordHash != passwordHash)
            return false;

        // Якщо у користувача немає/невалідний PersonalCode — генеруємо і зберігаємо
        if (!IsValidPersonalCode(user.PersonalCode))
        {
            user.PersonalCode = GeneratePersonalCode();
            await _userRepository.SaveAsync(user);
        }

        _currentUser = user;
        await _secureStorage.SetAsync("userId", user.Id.ToString());
        return true;
    }

    public async Task<bool> RegisterAsync(string email, string password)
    {
        var existingUser = await _userRepository.GetByEmailAsync(email);
        if (existingUser != null)
            return false;

        var user = new User
        {
            Email = email,
            PasswordHash = HashPassword(password),
            CreatedAt = DateTime.Now,
            PersonalCode = GeneratePersonalCode()   // 10-значний унікальний код
        };

        await _userRepository.SaveAsync(user);
        _currentUser = user;
        await _secureStorage.SetAsync("userId", user.Id.ToString());
        return true;
    }

    public async Task<bool> UpdateProfileAsync(
        string firstName, string lastName,
        DateTime? birthDate, string phoneNumber,
        string country,
        string avatarPath = null)
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return false;

        user.FirstName = firstName?.Trim();
        user.LastName = lastName?.Trim();
        user.BirthDate = birthDate;
        user.PhoneNumber = phoneNumber?.Trim();
        user.Country = country?.Trim();

        if (avatarPath != null)
            user.AvatarPath = avatarPath;

        // Якщо з якоїсь причини PersonalCode невалідний — відновлюємо
        if (!IsValidPersonalCode(user.PersonalCode))
            user.PersonalCode = GeneratePersonalCode();

        await _userRepository.SaveAsync(user);
        return true;
    }

    public async Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return false;

        if (user.PasswordHash != HashPassword(currentPassword))
            return false;

        user.PasswordHash = HashPassword(newPassword);
        await _userRepository.SaveAsync(user);
        return true;
    }

    public async Task LogoutAsync()
    {
        _currentUser = null;
        await _secureStorage.RemoveAsync("userId");
    }

    public async Task<User> GetCurrentUserAsync()
    {
        if (_currentUser != null)
            return _currentUser;

        var userIdStr = await _secureStorage.GetAsync("userId");
        if (int.TryParse(userIdStr, out int userId))
        {
            _currentUser = await _userRepository.GetByIdAsync(userId);
        }

        // Якщо у завантаженого користувача PersonalCode відсутній або невалідний
        // (наприклад БД була без цього поля, або збережено старий формат) —
        // генеруємо новий і одразу зберігаємо в БД.
        if (_currentUser != null && !IsValidPersonalCode(_currentUser.PersonalCode))
        {
            _currentUser.PersonalCode = GeneratePersonalCode();
            await _userRepository.SaveAsync(_currentUser);
        }

        return _currentUser;
    }

    public async Task<bool> DeleteAccountAsync()
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return false;

        try
        {
            var courses = await _courseRepository.GetByUserIdAsync(user.Id);
            foreach (var course in courses)
            {
                await _taskRepository.DeleteByCourseIdAsync(course.Id);
                await _courseRepository.DeleteAsync(course);
            }

            var allStats = await _statisticsRepository.GetAllAsync();
            var userStats = allStats.Where(s => s.UserId == user.Id).ToList();
            foreach (var stat in userStats)
                await _statisticsRepository.DeleteAsync(stat);

            await _userRepository.DeleteAsync(user);
            await LogoutAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    // Генерація персонального коду

    /// <summary>
    /// Генерує унікальний 10-значний числовий код.
    /// Діапазон: 1 000 000 000 – 9 999 999 999 (завжди рівно 10 цифр).
    /// </summary>
    private static string GeneratePersonalCode()
    {
        // NextInt64 дає рівномірний розподіл без побітових хаків
        long code = Random.Shared.NextInt64(1_000_000_000L, 10_000_000_000L);
        return code.ToString();
    }

    /// <summary>
    /// Перевіряє що PersonalCode відповідає формату: рівно 10 цифр.
    /// Відкидає null, порожній рядок, старий формат "SP-XXXX-XXXX" та ID "1"/"2" тощо.
    /// </summary>
    private static bool IsValidPersonalCode(string code)
        => !string.IsNullOrWhiteSpace(code)
           && code.Length == 10
           && code.All(char.IsDigit);

    // Хешування пароля

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}