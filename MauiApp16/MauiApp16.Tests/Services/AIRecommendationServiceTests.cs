using Xunit;
using MauiApp16.Services;
using MauiApp16.Models;

namespace MauiApp16.Tests.Services;

// AIRecommendationService не має залежностей через конструктор —
// він лише викликає зовнішній API Anthropic.
// Тестуємо публічний контракт інтерфейсу IAIRecommendationService.

public class AIRecommendationServiceTests
{
    private readonly AIRecommendationService _service;

    public AIRecommendationServiceTests()
    {
        _service = new AIRecommendationService();
    }

    // ──────────────────────────────────────────────
    // GenerateStudyPlanAsync — порожній API ключ
    // ──────────────────────────────────────────────

    [Fact]
    public async Task GenerateStudyPlanAsync_EmptyApiKey_ThrowsArgumentException()
    {
        // Arrange
        var request = new AIRecommendationRequest
        {
            SubjectName    = "Математика",
            TaskCount      = 5,
            DifficultyLevel = "Середній",
            StudyTime      = "2 години",
            SelectedTaskTypes = new List<string> { "Лабораторна" }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.GenerateStudyPlanAsync(request, ""));
    }

    [Fact]
    public async Task GenerateStudyPlanAsync_NullApiKey_ThrowsArgumentException()
    {
        // Arrange
        var request = new AIRecommendationRequest
        {
            SubjectName    = "Фізика",
            TaskCount      = 3,
            DifficultyLevel = "Легкий",
            StudyTime      = "1 година",
            SelectedTaskTypes = new List<string>()
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.GenerateStudyPlanAsync(request, null!));
    }

    [Fact]
    public async Task GenerateStudyPlanAsync_WhitespaceApiKey_ThrowsArgumentException()
    {
        // Arrange
        var request = new AIRecommendationRequest
        {
            SubjectName    = "Алгоритми",
            TaskCount      = 4,
            DifficultyLevel = "Складний",
            StudyTime      = "3 години",
            SelectedTaskTypes = new List<string> { "Реферат" }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.GenerateStudyPlanAsync(request, "   "));
    }

    // ──────────────────────────────────────────────
    // Конструктор — сервіс створюється без винятків
    // ──────────────────────────────────────────────

    [Fact]
    public void Constructor_NoArguments_CreatesInstance()
    {
        var service = new AIRecommendationService();
        Assert.NotNull(service);
    }

    // ──────────────────────────────────────────────
    // IAIRecommendationService — реалізація інтерфейсу
    // ──────────────────────────────────────────────

    [Fact]
    public void Service_ImplementsInterface()
    {
        Assert.IsAssignableFrom<IAIRecommendationService>(_service);
    }
}
