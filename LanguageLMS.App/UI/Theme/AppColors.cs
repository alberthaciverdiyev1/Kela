using Avalonia.Media;

namespace LanguageLMS.App.UI.Theme;

public static class AppColors
{
    public static readonly Color Primary = Color.Parse("#059669");
    public static readonly Color PrimaryLight = Color.Parse("#D1FAE5");
    public static readonly Color Canvas = Color.Parse("#F7FAF8");
    public static readonly Color Surface = Color.Parse("#FFFFFF");
    public static readonly Color Text = Color.Parse("#18352B");
    public static readonly Color Muted = Color.Parse("#6B7B73");
    public static readonly Color Border = Color.Parse("#E5EEE9");

    public static readonly IBrush PrimaryBrush = new SolidColorBrush(Primary);
    public static readonly IBrush PrimaryLightBrush = new SolidColorBrush(PrimaryLight);
    public static readonly IBrush CanvasBrush = new SolidColorBrush(Canvas);
    public static readonly IBrush SurfaceBrush = new SolidColorBrush(Surface);
    public static readonly IBrush TextBrush = new SolidColorBrush(Text);
    public static readonly IBrush MutedBrush = new SolidColorBrush(Muted);
    public static readonly IBrush BorderBrush = new SolidColorBrush(Border);
}
