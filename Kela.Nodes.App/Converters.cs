using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Kela.Nodes.App;

/// <summary>Vəziyyət tonunu rəngə çevirir (success/warning/danger/muted).</summary>
public sealed class ToneBrushConverter : IValueConverter
{
    public static readonly ToneBrushConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        "success" => new SolidColorBrush(Color.Parse("#12B76A")),
        "warning" => new SolidColorBrush(Color.Parse("#F79009")),
        "danger" => new SolidColorBrush(Color.Parse("#F04438")),
        _ => new SolidColorBrush(Color.Parse("#98A2B3"))
    };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>0..1 aralığındaki faizi 0..100-ə çevirir (ProgressBar üçün).</summary>
public sealed class FractionConverter : IValueConverter
{
    public static readonly FractionConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is double fraction ? Math.Clamp(fraction, 0, 1) * 100 : 0d;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
