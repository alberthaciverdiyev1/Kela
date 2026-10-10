using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using LanguageLMS.App.UI.Views.Dashboard;
using Microsoft.Extensions.DependencyInjection;

namespace LanguageLMS.App;

public sealed class App(IServiceProvider services) : Application
{
    public override void Initialize()
    {
        Styles.Add(new StyleInclude(new Uri("avares://LanguageLMS.App"))
        {
            Source = new Uri("avares://Avalonia.Themes.Fluent/FluentLight.axaml")
        });
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = services.GetRequiredService<DashboardWindow>();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
