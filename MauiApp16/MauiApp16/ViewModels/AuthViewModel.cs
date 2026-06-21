using System.Windows.Input;
using MauiApp16.Services;

namespace MauiApp16.ViewModels;

public class AuthViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private string _email;
    private string _password;
    private string _errorMessage;

    public AuthViewModel(IAuthService authService)
    {
        _authService = authService;
        Title = "Вхід";

        LoginCommand = new Command(async () => await LoginAsync(), () => !IsBusy);
        RegisterCommand = new Command(async () => await GoToRegistrationAsync(), () => !IsBusy);
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand LoginCommand { get; }
    public ICommand RegisterCommand { get; }

    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Заповніть всі поля";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var success = await _authService.LoginAsync(Email, Password);
            if (success)
            {
                // Увімкнути навігаційну панель
                if (Application.Current.MainPage is AppShell shell)
                {
                    shell.EnableFlyout();
                }
                await Shell.Current.GoToAsync("//DashboardPage");
            }
            else
            {
                ErrorMessage = "Невірний email або пароль";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Помилка: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Тепер просто переходить на RegistrationPage — без перевірки email/пароля.
    /// Вся реєстраційна форма (email, пароль, дані профілю) зосереджена там.
    /// </summary>
    private async Task GoToRegistrationAsync()
    {
        ErrorMessage = string.Empty;
        await Shell.Current.GoToAsync("RegistrationPage");
    }
}