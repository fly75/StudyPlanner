using MauiApp16.ViewModels;

namespace MauiApp16.Views;

public partial class RegistrationPage : ContentPage
{
    public RegistrationPage(RegistrationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    // Відкриває DatePicker при кліку на Label з датою
    private void OnRegBirthDateTapped(object sender, EventArgs e)
    {
        RegBirthDatePicker.Focus();
    }

    // Відкриває Picker при кліку на будь-яку частину поля "Країна"
    private void OnRegCountryTapped(object sender, EventArgs e)
    {
        RegCountryPicker.Focus();
    }
}