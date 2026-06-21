using Xunit;
using Moq;
using MauiApp16.ViewModels;
using MauiApp16.Services;

namespace MauiApp16.Tests.ViewModels;

public class AuthViewModelTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AuthViewModel _viewModel;

    public AuthViewModelTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _viewModel = new AuthViewModel(_authServiceMock.Object);
    }

    // ──────────────────────────────────────────────
    // LoginCommand — валідація порожніх полів
    // ──────────────────────────────────────────────

    [Fact]
    public void LoginCommand_EmptyEmail_SetsErrorMessage()
    {
        // Arrange
        _viewModel.Email = "";
        _viewModel.Password = "password123";

        // Act
        _viewModel.LoginCommand.Execute(null);

        // Assert
        Assert.Equal("Заповніть всі поля", _viewModel.ErrorMessage);
    }

    [Fact]
    public void LoginCommand_EmptyPassword_SetsErrorMessage()
    {
        // Arrange
        _viewModel.Email = "test@example.com";
        _viewModel.Password = "";

        // Act
        _viewModel.LoginCommand.Execute(null);

        // Assert
        Assert.Equal("Заповніть всі поля", _viewModel.ErrorMessage);
    }

    [Fact]
    public void LoginCommand_BothFieldsEmpty_SetsErrorMessage()
    {
        // Arrange
        _viewModel.Email = "";
        _viewModel.Password = "";

        // Act
        _viewModel.LoginCommand.Execute(null);

        // Assert
        Assert.Equal("Заповніть всі поля", _viewModel.ErrorMessage);
    }

    // ──────────────────────────────────────────────
    // LoginCommand — невірні облікові дані
    // ──────────────────────────────────────────────

    [Fact]
    public async Task LoginCommand_InvalidCredentials_SetsErrorMessage()
    {
        // Arrange
        _authServiceMock
            .Setup(s => s.LoginAsync("wrong@example.com", "wrongpass"))
            .ReturnsAsync(false);

        _viewModel.Email = "wrong@example.com";
        _viewModel.Password = "wrongpass";

        // Act
        _viewModel.LoginCommand.Execute(null);
        await Task.Delay(100); // дати час async-команді завершитися

        // Assert
        Assert.Equal("Невірний email або пароль", _viewModel.ErrorMessage);
    }

    // ──────────────────────────────────────────────
    // Властивості — прив'язка даних
    // ──────────────────────────────────────────────

    [Fact]
    public void Email_SetValue_PropertyChangedRaised()
    {
        // Arrange
        bool raised = false;
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_viewModel.Email))
                raised = true;
        };

        // Act
        _viewModel.Email = "student@university.ua";

        // Assert
        Assert.True(raised);
        Assert.Equal("student@university.ua", _viewModel.Email);
    }

    [Fact]
    public void Password_SetValue_PropertyChangedRaised()
    {
        // Arrange
        bool raised = false;
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_viewModel.Password))
                raised = true;
        };

        // Act
        _viewModel.Password = "securePass!1";

        // Assert
        Assert.True(raised);
        Assert.Equal("securePass!1", _viewModel.Password);
    }

    // ──────────────────────────────────────────────
    // Початковий стан ViewModel
    // ──────────────────────────────────────────────

    [Fact]
    public void Constructor_InitialState_CommandsNotNull()
    {
        Assert.NotNull(_viewModel.LoginCommand);
        Assert.NotNull(_viewModel.RegisterCommand);
    }

    [Fact]
    public void Constructor_InitialState_ErrorMessageEmpty()
    {
        Assert.True(string.IsNullOrEmpty(_viewModel.ErrorMessage));
    }
}
