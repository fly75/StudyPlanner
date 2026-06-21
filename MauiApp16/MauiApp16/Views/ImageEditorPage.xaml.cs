using SkiaSharp;

namespace MauiApp16.Views;

public partial class ImageEditorPage : ContentPage
{
    // Статичні поля для передачі даних між сторінками
    public static string SourceImagePath { get; set; }
    public static Action<string> OnSaved { get; set; }

    // Стан трансформацій
    private double _currentScale = 1.0;
    private double _startScale = 1.0;
    private double _xOffset = 0;
    private double _yOffset = 0;
    private double _panStartX = 0;
    private double _panStartY = 0;

    private const double MinScale = 0.5;
    private const double MaxScale = 5.0;
    private const double ZoomStep = 0.2;

    // Розмір кола кропу на екрані (має співпадати з WidthRequest/HeightRequest у XAML)
    private const float CropCircleScreenSize = 300f;

    public ImageEditorPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Скидаємо трансформації при кожному відкритті
        ResetTransform();
        LoadImage();
    }

    private void LoadImage()
    {
        if (!string.IsNullOrEmpty(SourceImagePath) && File.Exists(SourceImagePath))
        {
            EditImage.Source = ImageSource.FromFile(SourceImagePath);
        }
    }

    private void ResetTransform()
    {
        _currentScale = 1.0;
        _startScale = 1.0;
        _xOffset = 0;
        _yOffset = 0;
        EditImage.Scale = 1.0;
        EditImage.TranslationX = 0;
        EditImage.TranslationY = 0;
    }

    // Pinch (zoom)

    private void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
    {
        switch (e.Status)
        {
            case GestureStatus.Started:
                _startScale = _currentScale;
                break;

            case GestureStatus.Running:
                _currentScale = Math.Clamp(_startScale * e.Scale, MinScale, MaxScale);
                EditImage.Scale = _currentScale;
                break;
        }
    }

    // Pan (переміщення)

    private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _panStartX = _xOffset;
                _panStartY = _yOffset;
                break;

            case GestureStatus.Running:
                _xOffset = _panStartX + e.TotalX;
                _yOffset = _panStartY + e.TotalY;
                EditImage.TranslationX = _xOffset;
                EditImage.TranslationY = _yOffset;
                break;
        }
    }

    // Кнопки +/− масштабу

    private void OnZoomInClicked(object sender, EventArgs e)
    {
        _currentScale = Math.Min(_currentScale + ZoomStep, MaxScale);
        EditImage.Scale = _currentScale;
    }

    private void OnZoomOutClicked(object sender, EventArgs e)
    {
        _currentScale = Math.Max(_currentScale - ZoomStep, MinScale);
        EditImage.Scale = _currentScale;
    }

    private void OnResetClicked(object sender, EventArgs e)
    {
        ResetTransform();
    }

    // Кнопки Скасувати / Зберегти

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        // Очищаємо callback — скасування не має змінювати аватар
        OnSaved = null;
        ResetTransform();
        await Shell.Current.GoToAsync("..");
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(SourceImagePath) || !File.Exists(SourceImagePath))
        {
            await DisplayAlert("Помилка", "Зображення не знайдено", "OK");
            return;
        }

        try
        {
            SaveButton.IsEnabled = false;

            // Передаємо розміри відображуваної області для точного розрахунку
            var croppedPath = await CropImageAsync(
                SourceImagePath,
                (float)_currentScale,
                (float)_xOffset,
                (float)_yOffset,
                (float)EditImage.Width,
                (float)EditImage.Height,
                outputSize: 400);

            OnSaved?.Invoke(croppedPath);
            OnSaved = null;

            ResetTransform();
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка", $"Не вдалося зберегти: {ex.Message}", "OK");
        }
        finally
        {
            SaveButton.IsEnabled = true;
        }
    }

    // Обрізка через SkiaSharp (виправлена математика)

    /// <summary>
    /// Вирізає круглий аватар із урахуванням AspectFit-рендерингу та
    /// поточних трансформацій (масштаб + зсув).
    /// </summary>
    private static Task<string> CropImageAsync(
        string sourcePath,
        float userScale,    // масштаб, застосований жестом/кнопкою
        float offsetX,      // зсув в екранних пікселях
        float offsetY,
        float viewWidth,    // ширина Image-елементу в MAUI-юнітах
        float viewHeight,   // висота Image-елементу в MAUI-юнітах
        int outputSize)
    {
        return Task.Run(() =>
        {
            using var original = SKBitmap.Decode(sourcePath);
            if (original == null)
                throw new InvalidOperationException("Не вдалося завантажити зображення.");

            // 1. Розраховуємо як зображення відображається (AspectFit)
            float imgAspect = (float)original.Width / original.Height;
            float viewAspect = viewWidth / viewHeight;

            float displayWidth, displayHeight;
            if (imgAspect > viewAspect)
            {
                // Обмежено по ширині
                displayWidth = viewWidth;
                displayHeight = viewWidth / imgAspect;
            }
            else
            {
                // Обмежено по висоті
                displayHeight = viewHeight;
                displayWidth = viewHeight * imgAspect;
            }

            // 2. Коефіцієнт: скільки пікселів зображення на 1 MAUI-юніт без масштабу
            float imgPerUnit = original.Width / displayWidth;

            // 3. Радіус кола кропу в MAUI-юнітах та в пікселях зображення
            float circleRadiusUnits = CropCircleScreenSize / 2f;
            float circleRadiusImgPx = circleRadiusUnits / userScale * imgPerUnit;

            // 4. Центр кропу: центр view мінус зсув, перерахований в пікселі зображення
            float centerXUnits = viewWidth / 2f - offsetX / userScale;
            float centerYUnits = viewHeight / 2f - offsetY / userScale;

            // Зміщення відносно початку зображення в AspectFit (letterbox offset)
            float letterboxX = (viewWidth - displayWidth) / 2f;
            float letterboxY = (viewHeight - displayHeight) / 2f;

            float imgCenterX = (centerXUnits - letterboxX) * imgPerUnit;
            float imgCenterY = (centerYUnits - letterboxY) * imgPerUnit;

            // 5. Прямокутник вирізання в пікселях зображення
            var srcRect = new SKRect(
                imgCenterX - circleRadiusImgPx,
                imgCenterY - circleRadiusImgPx,
                imgCenterX + circleRadiusImgPx,
                imgCenterY + circleRadiusImgPx);

            // Клампуємо до меж зображення
            srcRect.Left = Math.Max(0, srcRect.Left);
            srcRect.Top = Math.Max(0, srcRect.Top);
            srcRect.Right = Math.Min(original.Width, srcRect.Right);
            srcRect.Bottom = Math.Min(original.Height, srcRect.Bottom);

            var destRect = new SKRect(0, 0, outputSize, outputSize);

            // 6. Малюємо на новому квадратному канвасі з круглою маскою
            using var surface = SKSurface.Create(new SKImageInfo(outputSize, outputSize));
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            using var circlePath = new SKPath();
            circlePath.AddCircle(outputSize / 2f, outputSize / 2f, outputSize / 2f);
            canvas.ClipPath(circlePath, SKClipOperation.Intersect, antialias: true);

            canvas.DrawBitmap(original, srcRect, destRect);

            using var snapshot = surface.Snapshot();
            using var encoded = snapshot.Encode(SKEncodedImageFormat.Png, 100); // PNG для прозорості

            var destFolder = Path.Combine(FileSystem.AppDataDirectory, "avatars");
            Directory.CreateDirectory(destFolder);
            var destPath = Path.Combine(destFolder, $"avatar_crop_{DateTime.Now:yyyyMMddHHmmss}.png");

            using var fs = File.OpenWrite(destPath);
            encoded.SaveTo(fs);

            return destPath;
        });
    }
}