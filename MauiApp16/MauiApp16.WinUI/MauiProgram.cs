using MauiApp16.Data;
using MauiApp16.Services;
using MauiApp16.ViewModels;
using MauiApp16.Views;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;

namespace MauiApp16;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseLocalNotification()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("materialdesignicons-webfont.ttf", "MaterialIcons");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Database
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "studyplanner.db3");
        builder.Services.AddSingleton(new DatabaseContext(dbPath));

        // Repositories
        builder.Services.AddSingleton<UserRepository>();
        builder.Services.AddSingleton<CourseRepository>();
        builder.Services.AddSingleton<TaskRepository>();
        builder.Services.AddSingleton<StatisticsRepository>();

        // Services
        builder.Services.AddSingleton<ISecureStorageService, SecureStorageService>();
        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton<ICourseService, CourseService>();
        builder.Services.AddSingleton<ITaskService, TaskService>();
        builder.Services.AddSingleton<IStatisticsService, StatisticsService>();
        builder.Services.AddSingleton<MauiApp16.Services.INotificationService, MauiApp16.Services.NotificationService>();
        builder.Services.AddSingleton<IMotivationService, MotivationService>();
        builder.Services.AddSingleton<ISettingsService, SettingsService>();
        builder.Services.AddSingleton<ITestDataService, TestDataService>();
        builder.Services.AddSingleton<IExportService, ExportService>();
        builder.Services.AddSingleton<IBackupService, BackupService>();
        builder.Services.AddSingleton<AppNotificationRepository>();
        builder.Services.AddSingleton<IInAppNotificationService, InAppNotificationService>();
        builder.Services.AddSingleton<IAIRecommendationService, AIRecommendationService>();

        // ViewModels
        builder.Services.AddTransient<AuthViewModel>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<CoursesViewModel>();
        builder.Services.AddTransient<TasksViewModel>();
        builder.Services.AddTransient<TaskDetailViewModel>();
        builder.Services.AddTransient<StatisticsViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<CalendarViewModel>();
        builder.Services.AddTransient<RegistrationViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<RecommendationsViewModel>();
        builder.Services.AddTransient<HelpViewModel>();

        // Views
        builder.Services.AddTransient<AuthorizationPage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<CoursesPage>();
        builder.Services.AddTransient<TasksPage>();
        builder.Services.AddTransient<TaskDetailPage>();
        builder.Services.AddTransient<StatisticsPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<CalendarPage>();
        builder.Services.AddTransient<RegistrationPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<ImageEditorPage>();
        builder.Services.AddTransient<RecommendationsPage>();
        builder.Services.AddTransient<HelpPage>();

        return builder.Build();
    }
}