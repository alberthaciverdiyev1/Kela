using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Avalonia.Threading;
using Edutor.Nodes.App.ViewModels;
using Edutor.Nodes.App.Views;

namespace Edutor.Nodes.App.Platforms.Desktop;

/// <summary>
/// Masaüstü davranışı: pəncərə bağlandıqda tətbiq dayanmır, treyə keçir və
/// konversiya davam edir. Trey dəstəklənməyən mühitdə normal bağlanma qalır.
/// </summary>
public sealed class DesktopShell : IDisposable
{
    private readonly IClassicDesktopStyleApplicationLifetime _desktop;
    private readonly MainViewModel _model;
    private readonly MainWindow _window;
    private readonly TrayIcon? _tray;
    private readonly NativeMenuItem _toggleItem;
    private bool _exiting;

    /// <summary>Tətbiq açılanda bir dəfə çağırılır: trey, tək nüsxə və gizli başlanğıc.</summary>
    public static DesktopShell Attach(
        IClassicDesktopStyleApplicationLifetime desktop, MainWindow window, MainViewModel model, bool startHidden)
    {
        var shell = new DesktopShell(desktop, window, model);

        PlatformExtras.StartupRegistration = StartupRegistration.Apply;
        SingleInstance.Listen(() => Dispatcher.UIThread.Post(shell.ShowWindow));

        if (startHidden && shell.HasTray) shell.StartHidden();

        return shell;
    }

    private DesktopShell(IClassicDesktopStyleApplicationLifetime desktop, MainWindow window, MainViewModel model)
    {
        _desktop = desktop;
        _window = window;
        _model = model;

        _toggleItem = new NativeMenuItem { Header = ToggleHeader() };
        _toggleItem.Click += (_, _) => ToggleEngine();

        _tray = TryCreateTray();

        // Trey varsa pəncərə bağlananda tətbiq yaşayır; yoxsa bağlanma çıxış deməkdir.
        if (_tray is null) return;

        desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        _window.Closing += OnClosing;
    }

    public bool HasTray => _tray is not null;

    public void ShowWindow()
    {
        if (_exiting) return;

        _window.Show();

        if (_window.WindowState == WindowState.Minimized) _window.WindowState = WindowState.Normal;

        _window.Activate();
    }

    public void StartHidden()
    {
        if (_tray is null) return;
        _window.Hide();
    }

    /// <summary>Tətbiqi tamamilə bağlayır — trey menyusundan çağırılır.</summary>
    public async void Exit()
    {
        if (_exiting) return;
        _exiting = true;

        try
        {
            await _model.Engine.StopAsync();
        }
        catch (Exception ex) when (ex is IOException or ObjectDisposedException)
        {
            // Motor artıq dayanıbsa çıxışa mane olmağa dəyməz.
        }

        _desktop.Shutdown();
    }

    public void Dispose() => _tray?.Dispose();

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (_exiting) return;

        // Pəncərə yalnız gizlədilir: encoding arxa planda qalır.
        e.Cancel = true;
        _window.Hide();
    }

    private void ToggleEngine()
    {
        _ = Dispatcher.UIThread.InvokeAsync(async () =>
        {
            if (_model.Engine.IsRunning) await _model.Engine.StopAsync();
            else await _model.Engine.StartAsync();

            _toggleItem.Header = ToggleHeader();
            _model.Refresh();
        });
    }

    private string ToggleHeader() => _model.Engine.IsRunning ? "Dayandır" : "İşə sal";

    private TrayIcon? TryCreateTray()
    {
        try
        {
            var open = new NativeMenuItem("Pəncərəni aç");
            open.Click += (_, _) => ShowWindow();

            var exit = new NativeMenuItem("Çıxış");
            exit.Click += (_, _) => Exit();

            var menu = new NativeMenu();
            menu.Add(open);
            menu.Add(new NativeMenuItemSeparator());
            menu.Add(_toggleItem);
            menu.Add(new NativeMenuItemSeparator());
            menu.Add(exit);

            var tray = new TrayIcon
            {
                ToolTipText = "Edutor Nodes",
                Menu = menu,
                IsVisible = true
            };

            try
            {
                tray.Icon = new WindowIcon(AssetLoader.Open(new Uri("avares://Edutor.Nodes.App/Assets/appicon.png")));
            }
            catch (Exception ex) when (ex is IOException or ArgumentException)
            {
                // İkon yüklənməzsə trey onsuz da işləyir.
            }

            tray.Clicked += (_, _) => ShowWindow();
            return tray;
        }
        catch (Exception)
        {
            // Bəzi Linux masaüstlərində trey backend-i yoxdur — pəncərə rejimi qalır.
            return null;
        }
    }
}
