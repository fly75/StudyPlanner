using MauiApp16.ViewModels;

namespace MauiApp16.Views;

public partial class ProfilePage : ContentPage
{
    private readonly ProfileViewModel _viewModel;

    public ProfilePage(ProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }

    // Відкриває Picker при кліку на Frame або стрілку ?
    private void OnProfCountryTapped(object sender, EventArgs e)
    {
        ProfCountryPicker.Focus();
    }
}