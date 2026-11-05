using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Edutor.Nodes.App.ViewModels;
using Edutor.Nodes.App.Views;
#if !ANDROID
using Edutor.Nodes.App.Platforms.Desktop;
#endif

namespace Edutor.Nodes.App;

public sealed class App : Avalonia.Application
{
    /// <summary>Sistem açılışında başladıqda pəncərə göstərilmir, yalnız trey görünür.</summary>
    public static bool StartHidden { get; set; }

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        var model = new MainViewModel();

        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                var window = new MainWindow { DataContext = model };
                desktop.MainWindow = window;
                // Bağlanma bloklanmasın: dayandırma üçün sonlu vaxt verilir.
                desktop.ShutdownRequested += (_, _) => model.Engine.StopAsync().Wait(TimeSpan.FromSeconds(5));

#if ANDROID
                break;
#else
                DesktopShell.Attach(desktop, window, model, StartHidden);
                StartupRegistration.Apply(model.Settings.RunAtStartup);
                break;
#endif

            // Android və iOS tək görünüş işlədir: Window yox, birbaşa idarə elementi verilir.
            case ISingleViewApplicationLifetime singleView:
                singleView.MainView = new MainView { DataContext = model };
                break;
        }

        // "Avtomatik başlat" seçilibsə və cihaz qeydə alınıbsa, worker dərhal qoşulur.
        WorkerHost.TryAutoStart();

        base.OnFrameworkInitializationCompleted();
    }
}
