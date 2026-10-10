using Avalonia;
using LanguageLMS.App.Configuration;

namespace LanguageLMS.App.Platform.Desktop;

public static class DesktopHost
{
    public static AppBuilder BuildAvaloniaApp()
    {
        var services = AppBootstrapper.BuildServices();

        return AppBuilder.Configure(() => new App(services))
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }
}
