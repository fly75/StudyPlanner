using MauiApp16.ViewModels;

namespace MauiApp16.Views;

public partial class RecommendationsPage : ContentPage
{
    private readonly RecommendationsViewModel _viewModel;

    public RecommendationsPage(RecommendationsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;

        // Підписуємось на подію тостів з ViewModel
        _viewModel.ShowToastRequested += (icon, message) =>
            MainThread.BeginInvokeOnMainThread(() =>
                _ = ShowToastAsync(icon, message));
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Показуємо перший тост при відкритті сторінки
        _ = Task.Delay(800).ContinueWith(_ =>
            MainThread.BeginInvokeOnMainThread(() =>
                _ = ShowToastAsync("ℹ️",
                    "Заповніть опитування, і AI створить план спеціально для вас")));
    }

    // Поділитися файлом
    private async void OnShareFileClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_viewModel.GeneratedFilePath)) return;
        if (!File.Exists(_viewModel.GeneratedFilePath)) return;

        try
        {
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "AI Навчальний план",
                File = new ShareFile(_viewModel.GeneratedFilePath)
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка", ex.Message, "OK");
        }
    }

    //  СИСТЕМА ТОСТІВ
    private async Task ShowToastAsync(string icon, string message)
    {
        var toast = BuildToastFrame(icon, message);
        ToastContainer.Children.Add(toast);

        // Slide in з правої сторони
        toast.TranslationX = 290;
        toast.Opacity = 0;

        await Task.WhenAll(
            toast.TranslateTo(0, 0, 320, Easing.CubicOut),
            toast.FadeTo(1.0, 320));

        // Автоматичне зникнення через 16 секунд
        var cts = new CancellationTokenSource();
        // Зберігаємо cts у Tag для можливості скасування
        toast.Resources["ToastCts"] = cts;

        _ = Task.Delay(16_000, cts.Token).ContinueWith(async t =>
        {
            if (!t.IsCanceled)
                await DismissToastAsync(toast);
        }, TaskScheduler.Default);
    }

    private async Task DismissToastAsync(View toast)
    {
        if (!MainThread.IsMainThread)
        {
            await MainThread.InvokeOnMainThreadAsync(() => DismissToastAsync(toast));
            return;
        }

        if (!ToastContainer.Children.Contains(toast)) return;

        // Скасовуємо таймер якщо є
        if (toast.Resources.TryGetValue("ToastCts", out var res) && res is CancellationTokenSource cts)
            cts.Cancel();

        await Task.WhenAll(
            toast.TranslateTo(290, 0, 250, Easing.CubicIn),
            toast.FadeTo(0, 250));

        ToastContainer.Children.Remove(toast);
    }

    private Frame BuildToastFrame(string icon, string message)
    {
        // Кнопка закриття
        var closeLabel = new Label
        {
            Text = "✕",
            FontSize = 15,
            TextColor = GetResourceColor("TextSecondary"),
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Start,
            Margin = new Thickness(0, -2, 0, 0),
            WidthRequest = 24,
            HeightRequest = 24
        };

        // Іконка
        var iconLabel = new Label
        {
            Text = icon,
            FontSize = 20,
            VerticalOptions = LayoutOptions.Start
        };

        // Текст повідомлення
        var msgLabel = new Label
        {
            Text = message,
            FontSize = 12,
            TextColor = GetResourceColor("TextPrimary"),
            LineBreakMode = LineBreakMode.WordWrap,
            MaxLines = 4
        };

        // Компонування
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = new GridLength(28) }
            },
            ColumnSpacing = 8
        };

        grid.Add(iconLabel, 0, 0);
        grid.Add(msgLabel, 1, 0);
        grid.Add(closeLabel, 2, 0);

        var frame = new Frame
        {
            BackgroundColor = GetResourceColor("Surface"),
            CornerRadius = 14,
            HasShadow = true,
            Padding = new Thickness(14, 12),
            Content = grid,
            BorderColor = GetResourceColor("Primary")
        };

        // Жест закриття
        var closeTap = new TapGestureRecognizer();
        closeTap.Tapped += async (_, _) => await DismissToastAsync(frame);
        closeLabel.GestureRecognizers.Add(closeTap);
        frame.GestureRecognizers.Add(closeTap);

        return frame;
    }

    private Color GetResourceColor(string key)
    {
        if (Application.Current?.Resources?.TryGetValue(key, out var value) == true
            && value is Color color)
            return color;
        return Colors.Gray;
    }
}