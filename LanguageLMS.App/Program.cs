using Avalonia;
using LanguageLMS.App.Platform.Desktop;

namespace LanguageLMS.App;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        DesktopHost.BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }
}
