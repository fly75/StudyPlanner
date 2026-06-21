using MauiApp16.ViewModels;

namespace MauiApp16.Views;

public partial class DashboardPage : ContentPage
{
    private readonly DashboardViewModel _viewModel;

    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }

    // Help
    private async void OnHelpIconTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("HelpPage");
    }

    // QR-код
    private async void OnQrIconTapped(object sender, TappedEventArgs e)
    {
        QrPopupFrame.Scale = 0.8;
        QrPopupFrame.Opacity = 0;
        QrOverlay.IsVisible = true;

        await Task.WhenAll(
            QrPopupFrame.ScaleTo(1.0, 250, Easing.CubicOut),
            QrPopupFrame.FadeTo(1.0, 250));
    }

    private async void OnQrCloseClicked(object sender, EventArgs e)
        => await HideQrOverlayAsync();

    private async void OnQrOverlayBackgroundTapped(object sender, TappedEventArgs e)
    {
        // Закриваємо лише якщо тапнули фон, не сам popup
        if (sender is Grid)
            await HideQrOverlayAsync();
    }

    private async Task HideQrOverlayAsync()
    {
        await Task.WhenAll(
            QrPopupFrame.ScaleTo(0.8, 200, Easing.CubicIn),
            QrPopupFrame.FadeTo(0, 200));
        QrOverlay.IsVisible = false;
    }
}