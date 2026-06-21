using System.Globalization;

namespace MauiApp16.Converters
{
    /// <summary>
    /// Повертає TrueValue якщо значення == true, інакше FalseValue.
    /// Використання в XAML:
    ///   &lt;converters:BoolToStringConverter TrueValue="Так" FalseValue="Ні"/&gt;
    /// </summary>
    public class BoolToStringConverter : IValueConverter
    {
        public string TrueValue { get; set; } = "True";
        public string FalseValue { get; set; } = "False";

        public object Convert(object? value, Type targetType,
                              object? parameter, CultureInfo culture)
            => value is true ? TrueValue : FalseValue;

        public object ConvertBack(object? value, Type targetType,
                                  object? parameter, CultureInfo culture)
            => value?.ToString() == TrueValue;
    }
}
