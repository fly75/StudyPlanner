using Xunit;
using Moq;
using MauiApp16.ViewModels;
using MauiApp16.Services;

namespace MauiApp16.Tests.ViewModels;

public class RegistrationViewModelTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly RegistrationViewModel _viewModel;

    public RegistrationViewModelTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _viewModel = new RegistrationViewModel(_authServiceMock.Object);
    }

    // ──────────────────────────────────────────────
    // Валідація — порожні обов'язкові поля
    // ──────────────────────────────────────────────

    [Fact]
    public void RegisterCommand_EmptyEmail_SetsValidationError()
    {
        // Arrange
        _viewModel.Email = "";
        _viewModel.Password = "Pass123!";
        _viewModel.ConfirmPassword = "Pass123!";

        // Act
        _viewModel.RegisterCommand.Execute(null);

        // Assert
        Assert.False(string.IsNullOrEmpty(_viewModel.ErrorMessage));
    }

    [Fact]
    public void RegisterCommand_PasswordMismatch_SetsError()
    {
        // Arrange
        _viewModel.Email = "new@student.ua";
        _viewModel.Password = "Pass123!";
        _viewModel.ConfirmPassword = "Different456!";

        // Act
        _viewModel.RegisterCommand.Execute(null);

        // Assert
        Assert.False(string.IsNullOrEmpty(_viewModel.ErrorMessage));
    }

    // ──────────────────────────────────────────────
    // Початковий стан
    // ──────────────────────────────────────────────

    [Fact]
    public void Constructor_InitialBirthDate_IsAdult()
    {
        // BirthDate за замовчуванням — DateTime.Today.AddYears(-18)
        var expected = DateTime.Today.AddYears(-18);
        Assert.Equal(expected.Date, _viewModel.BirthDate.Date);
    }

    [Fact]
    public void Constructor_InitialCountry_IsOther()
    {
        Assert.Equal("Інше", _viewModel.Country);
    }

    [Fact]
    public void Constructor_Commands_NotNull()
    {
        Assert.NotNull(_viewModel.RegisterCommand);
        Assert.NotNull(_viewModel.PickAvatarCommand);
        Assert.NotNull(_viewModel.BackCommand);
    }

    // ──────────────────────────────────────────────
    // Властивості — прив'язка даних
    // ──────────────────────────────────────────────

    [Theory]
    [InlineData("Іван")]
    [InlineData("")]
    [InlineData("Олександра-Вікторія")]
    public void FirstName_SetValue_ReturnsCorrectValue(string name)
    {
        _viewModel.FirstName = name;
        Assert.Equal(name, _viewModel.FirstName);
    }

    [Theory]
    [InlineData("Шевченко")]
    [InlineData("")]
    public void LastName_SetValue_ReturnsCorrectValue(string lastName)
    {
        _viewModel.LastName = lastName;
        Assert.Equal(lastName, _viewModel.LastName);
    }
}
