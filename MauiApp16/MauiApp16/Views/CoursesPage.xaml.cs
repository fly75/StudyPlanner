using MauiApp16.ViewModels;

namespace MauiApp16.Views;

public partial class CoursesPage : ContentPage
{
    private readonly CoursesViewModel _viewModel;

    public CoursesPage(CoursesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Завжди перезавантажуємо курси при появі сторінки
        // Це оновить прогрес після змін в TasksPage
        await _viewModel.LoadCoursesAsync();
    }
}