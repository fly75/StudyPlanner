using MauiApp16.ViewModels;

namespace MauiApp16.Views;

public partial class AuthorizationPage : ContentPage
{
    public AuthorizationPage(AuthViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}