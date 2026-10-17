using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Kela.Nodes.App.ViewModels;
using Kela.Nodes.App.Views;

namespace Kela.Nodes.App;

public sealed class App : Avalonia.Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        var model = new MainViewModel();

        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                desktop.MainWindow = new MainWindow { DataContext = model };
                desktop.ShutdownRequested += (_, _) => model.Engine.StopAsync().GetAwaiter().GetResult();
                break;

            // Android və iOS tək görünüş işlədir: Window yox, birbaşa idarə elementi verilir.
            case ISingleViewApplicationLifetime singleView:
                singleView.MainView = new MainView { DataContext = model };
                break;
        }

        // "Avtomatik başlat" seçilibsə və cihaz qeydə alınıbsa, worker dərhal qoşulur.
        if (model.Settings.AutoStart && model.Settings.IsRegistered)
        {
            _ = model.Engine.StartAsync();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
