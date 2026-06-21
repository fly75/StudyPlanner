using MauiApp16.ViewModels;

namespace MauiApp16.Views;

public partial class TasksPage : ContentPage
{
    private readonly TasksViewModel _viewModel;

    public TasksPage(TasksViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;

        // Підписуємось на події після ініціалізації
        AttachButtonAnimations();
    }

    private void AttachButtonAnimations()
    {
        // ─── Кнопка фільтрів ───────────────────────────────────────────
        var filterTap = new TapGestureRecognizer();
        filterTap.Tapped += async (s, e) =>
        {
            await AnimatePressAsync(FilterButton);
            _viewModel.ToggleFilterPanelCommand.Execute(null);
        };

        // Замінюємо існуючий TapGestureRecognizer щоб не було подвійного виклику
        FilterButton.GestureRecognizers.Clear();
        FilterButton.GestureRecognizers.Add(filterTap);

        // Hover для Windows/Desktop
        var filterPointer = new PointerGestureRecognizer();
        filterPointer.PointerEntered += (s, e) => _ = AnimateHoverEnterAsync(FilterButton);
        filterPointer.PointerExited += (s, e) => _ = AnimateHoverExitAsync(FilterButton);
        FilterButton.GestureRecognizers.Add(filterPointer);

        // ─── Кнопка експорту ───────────────────────────────────────────
        var exportTap = new TapGestureRecognizer();
        exportTap.Tapped += async (s, e) =>
        {
            await AnimatePressAsync(ExportButton);
            await HandleExportAsync();
        };

        ExportButton.GestureRecognizers.Clear();
        ExportButton.GestureRecognizers.Add(exportTap);

        var exportPointer = new PointerGestureRecognizer();
        exportPointer.PointerEntered += (s, e) => _ = AnimateHoverEnterAsync(ExportButton);
        exportPointer.PointerExited += (s, e) => _ = AnimateHoverExitAsync(ExportButton);
        ExportButton.GestureRecognizers.Add(exportPointer);
    }

    // ─── Анімація натискання (scale down → up) ─────────────────────────────
    private static async Task AnimatePressAsync(View view)
    {
        await view.ScaleTo(0.88, 80, Easing.CubicIn);
        await view.ScaleTo(1.0, 120, Easing.SpringOut);
    }

    // ─── Анімація наведення (hover enter) ─────────────────────────────────
    private static async Task AnimateHoverEnterAsync(View view)
    {
        await Task.WhenAll(
            view.ScaleTo(1.08, 150, Easing.CubicOut),
            view.FadeTo(0.85, 150)
        );
    }

    // ─── Анімація виходу курсора (hover exit) ─────────────────────────────
    private static async Task AnimateHoverExitAsync(View view)
    {
        await Task.WhenAll(
            view.ScaleTo(1.0, 150, Easing.CubicOut),
            view.FadeTo(1.0, 150)
        );
    }

    // ─── Логіка експорту (винесена з OnExportTapped) ──────────────────────
    private async Task HandleExportAsync()
    {
        var format = await DisplayActionSheet(
            "Експортувати завдання",
            "Скасувати",
            null,
            "📊 CSV (Excel)",
            "🗂 JSON",
            "📄 TXT (Звіт)");

        switch (format)
        {
            case "📊 CSV (Excel)": _viewModel.ExportCsvCommand.Execute(null); break;
            case "🗂 JSON": _viewModel.ExportJsonCommand.Execute(null); break;
            case "📄 TXT (Звіт)": _viewModel.ExportTxtCommand.Execute(null); break;
        }
    }

    // ─── Решта методів без змін ────────────────────────────────────────────
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel.SelectedCourse != null)
            await _viewModel.LoadTasksAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
    }
}