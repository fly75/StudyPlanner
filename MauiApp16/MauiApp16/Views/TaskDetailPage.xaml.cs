using MauiApp16.ViewModels;

namespace MauiApp16.Views;

public partial class TaskDetailPage : ContentPage
{
    public TaskDetailPage(TaskDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}