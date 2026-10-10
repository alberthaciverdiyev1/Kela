using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using LanguageLMS.App.UI.Theme;

namespace LanguageLMS.App.UI.Components;

public sealed class PrimaryButton : Button
{
    public PrimaryButton()
    {
        Background = AppColors.PrimaryBrush;
        Foreground = Brushes.White;
        CornerRadius = new CornerRadius(10);
        Padding = new Thickness(16, 10);
        FontWeight = FontWeight.SemiBold;
        BorderThickness = new Thickness(0);
    }
}
