namespace MauiApp16.Behaviors;

public class ProgressBarBehavior : Behavior<Frame>
{
    public static readonly BindableProperty ProgressProperty =
        BindableProperty.Create(nameof(Progress), typeof(double), typeof(ProgressBarBehavior), 0.0, propertyChanged: OnProgressChanged);

    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    private Frame? _progressFrame;
    private Frame? _containerFrame;

    protected override void OnAttachedTo(Frame bindable)
    {
        base.OnAttachedTo(bindable);
        _containerFrame = bindable;
        _containerFrame.SizeChanged += OnSizeChanged;

        // Знаходимо внутрішній Frame (прогрес)
        if (_containerFrame.Content is Grid grid && grid.Children.Count > 0)
        {
            _progressFrame = grid.Children[0] as Frame;
        }
    }

    protected override void OnDetachingFrom(Frame bindable)
    {
        if (_containerFrame != null)
        {
            _containerFrame.SizeChanged -= OnSizeChanged;
        }
        base.OnDetachingFrom(bindable);
    }

    private void OnSizeChanged(object? sender, EventArgs e)
    {
        UpdateProgress();
    }

    private static void OnProgressChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ProgressBarBehavior behavior)
        {
            behavior.UpdateProgress();
        }
    }

    private void UpdateProgress()
    {
        if (_containerFrame == null || _progressFrame == null)
            return;

        var containerWidth = _containerFrame.Width;
        if (containerWidth <= 0)
            return;

        // Встановлюємо ширину прогрес-бару відповідно до прогресу
        var progressWidth = (Progress / 100.0) * containerWidth;
        _progressFrame.WidthRequest = Math.Max(0, progressWidth);
        _progressFrame.IsVisible = Progress > 0;
    }
}