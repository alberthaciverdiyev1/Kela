using Avalonia;
using Edutor.Nodes.App.Platforms.Desktop;

namespace Edutor.Nodes.App;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Sistem açılışında arxa planda işə düşəndə pəncərə açılmır.
        App.StartHidden = args.Contains("--minimized") || args.Contains("--tray");

        if (SingleInstance.TryActivateExisting()) return;

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
