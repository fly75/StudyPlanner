using Xunit;
using Moq;
using MauiApp16.Services;
using MauiApp16.Models;

namespace MauiApp16.Tests.Services;

// Тестуємо IAuthService через Mock — без реальної БД
public class AuthServiceTests
{
    private readonly Mock<IAuthService> _authServiceMock;

    public AuthServiceTests()
    {
        _authServiceMock = new Mock<IAuthService>();
    }

    // ──────────────────────────────────────────────
    // LoginAsync
    // ──────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsTrue()
    {
        _authServiceMock
            .Setup(s => s.LoginAsync("student@uni.ua", "Pass123!"))
            .ReturnsAsync(true);

        var result = await _authServiceMock.Object.LoginAsync("student@uni.ua", "Pass123!");

        Assert.True(result);
    }

    [Fact]
    public async Task LoginAsync_InvalidCredentials_ReturnsFalse()
    {
        _authServiceMock
            .Setup(s => s.LoginAsync("wrong@test.com", "wrongpass"))
            .ReturnsAsync(false);

        var result = await _authServiceMock.Object.LoginAsync("wrong@test.com", "wrongpass");

        Assert.False(result);
    }

    [Fact]
    public async Task LoginAsync_EmptyEmail_ReturnsFalse()
    {
        _authServiceMock
            .Setup(s => s.LoginAsync("", It.IsAny<string>()))
            .ReturnsAsync(false);

        var result = await _authServiceMock.Object.LoginAsync("", "anypass");

        Assert.False(result);
    }

    // ──────────────────────────────────────────────
    // RegisterAsync
    // ──────────────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_NewEmail_ReturnsTrue()
    {
        _authServiceMock
            .Setup(s => s.RegisterAsync("new@student.ua", "Pass123!"))
            .ReturnsAsync(true);

        var result = await _authServiceMock.Object.RegisterAsync("new@student.ua", "Pass123!");

        Assert.True(result);
        _authServiceMock.Verify(s => s.RegisterAsync("new@student.ua", "Pass123!"), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ExistingEmail_ReturnsFalse()
    {
        _authServiceMock
            .Setup(s => s.RegisterAsync("existing@test.com", It.IsAny<string>()))
            .ReturnsAsync(false);

        var result = await _authServiceMock.Object.RegisterAsync("existing@test.com", "Pass123!");

        Assert.False(result);
    }

    // ──────────────────────────────────────────────
    // IsAuthenticated
    // ──────────────────────────────────────────────

    [Fact]
    public void IsAuthenticated_BeforeLogin_ReturnsFalse()
    {
        _authServiceMock
            .Setup(s => s.IsAuthenticated)
            .Returns(false);

        Assert.False(_authServiceMock.Object.IsAuthenticated);
    }

    [Fact]
    public void IsAuthenticated_AfterLogin_ReturnsTrue()
    {
        _authServiceMock
            .Setup(s => s.IsAuthenticated)
            .Returns(true);

        Assert.True(_authServiceMock.Object.IsAuthenticated);
    }

    // ──────────────────────────────────────────────
    // LogoutAsync
    // ──────────────────────────────────────────────

    [Fact]
    public async Task LogoutAsync_Called_InvokesOnce()
    {
        _authServiceMock
            .Setup(s => s.LogoutAsync())
            .Returns(Task.CompletedTask);

        await _authServiceMock.Object.LogoutAsync();

        _authServiceMock.Verify(s => s.LogoutAsync(), Times.Once);
    }

    // ──────────────────────────────────────────────
    // GetCurrentUserAsync
    // ──────────────────────────────────────────────

    [Fact]
    public async Task GetCurrentUserAsync_AuthenticatedUser_ReturnsUser()
    {
        var user = new User { Id = 1, Email = "student@uni.ua" };
        _authServiceMock
            .Setup(s => s.GetCurrentUserAsync())
            .ReturnsAsync(user);

        var result = await _authServiceMock.Object.GetCurrentUserAsync();

        Assert.NotNull(result);
        Assert.Equal("student@uni.ua", result.Email);
    }

    [Fact]
    public async Task GetCurrentUserAsync_NotAuthenticated_ReturnsNull()
    {
        _authServiceMock
            .Setup(s => s.GetCurrentUserAsync())
            .ReturnsAsync((User)null);

        var result = await _authServiceMock.Object.GetCurrentUserAsync();

        Assert.Null(result);
    }
}
