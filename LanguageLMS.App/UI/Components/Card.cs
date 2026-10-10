using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using LanguageLMS.App.UI.Theme;

namespace LanguageLMS.App.UI.Components;

public sealed class Card : Border
{
    public Card()
    {
        Background = AppColors.SurfaceBrush;
        BorderBrush = AppColors.BorderBrush;
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(16);
        Padding = new Thickness(18);
        BoxShadow = new BoxShadows(new BoxShadow
        {
            Color = Color.FromArgb(18, 24, 53, 43),
            Blur = 26,
            OffsetY = 10
        });
    }
}
