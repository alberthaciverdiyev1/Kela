using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Kela.Nodes.App.ViewModels;
using Kela.Nodes.App.Views;

namespace Kela.Nodes.App;

// Android başlığı 'Android.App' namespace-ini qlobal using kimi əlavə edir,
// ona görə baza sinif tam adla göstərilir.
public sealed class App : Avalonia.Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var model = new MainViewModel();

            desktop.MainWindow = new MainWindow { DataContext = model };
            desktop.ShutdownRequested += (_, _) => model.Engine.StopAsync().GetAwaiter().GetResult();

            // "Avtomatik başlat" seçilibsə və cihaz qeydə alınıbsa, worker dərhal qoşulur.
            if (model.Settings.AutoStart && model.Settings.IsRegistered)
            {
                _ = model.Engine.StartAsync();
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}
