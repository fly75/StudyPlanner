using MauiApp16.Views;

namespace MauiApp16;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("TasksPage", typeof(Views.TasksPage));
        Routing.RegisterRoute("TaskDetailPage", typeof(Views.TaskDetailPage));
        Routing.RegisterRoute("CoursesPage", typeof(Views.CoursesPage));
        Routing.RegisterRoute("RegistrationPage", typeof(Views.RegistrationPage));
        Routing.RegisterRoute("ProfilePage", typeof(Views.ProfilePage));
        Routing.RegisterRoute("ImageEditorPage", typeof(Views.ImageEditorPage));
        Routing.RegisterRoute("RecommendationsPage", typeof(Views.RecommendationsPage));
        Routing.RegisterRoute("HelpPage", typeof(Views.HelpPage));
    }

    public void EnableFlyout()
    {
        Shell.SetFlyoutBehavior(this, FlyoutBehavior.Flyout);
    }

    public void DisableFlyout()
    {
        Shell.SetFlyoutBehavior(this, FlyoutBehavior.Disabled);
    }

    public void UpdateThemeColors()
    {
        if (Application.Current?.Resources == null)
            return;

        // Перевіряємо чи це темна тема по кольору тексту
        bool isDarkTheme = false;

        if (Application.Current.Resources.TryGetValue("TextPrimary", out var textColor))
        {
            var color = (Color)textColor;
            // Якщо текст світлий (близький до білого), то це темна тема
            isDarkTheme = color.Red > 0.5 && color.Green > 0.5 && color.Blue > 0.5;
        }

        // Отримуємо актуальні кольори з ресурсів
        var backgroundColor = (Color)Application.Current.Resources["Background"];
        var foregroundColor = (Color)Application.Current.Resources["TextPrimary"];

        // Оновлюємо кольори Shell
        this.FlyoutBackgroundColor = backgroundColor;
        this.BackgroundColor = backgroundColor;

        // Оновлюємо колір тексту в меню
        Shell.SetForegroundColor(this, Colors.White);
        Shell.SetTitleColor(this, foregroundColor);

        // Примусово оновлюємо всі FlyoutItems
        MainThread.BeginInvokeOnMainThread(() =>
        {
            // Тригер перерисовки меню
            var currentBehavior = Shell.GetFlyoutBehavior(this);
            Shell.SetFlyoutBehavior(this, FlyoutBehavior.Disabled);
            Shell.SetFlyoutBehavior(this, currentBehavior);
        });
    }
}