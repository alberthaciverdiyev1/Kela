using Android.App;
using Android.Content.PM;
using Android.OS;
using Avalonia;
using Avalonia.Android;

namespace Kela.Nodes.App.Platforms.Android;

[Activity(
    Label = "Kela Nodes",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public sealed class MainActivity : AvaloniaMainActivity<Kela.Nodes.App.App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
        => base.CustomizeAppBuilder(builder).WithInterFont();

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        // Avalonia tətbiqi base.OnCreate daxilində qurulur, ona görə platforma
        // imkanları oradan ƏVVƏL qeydiyyatdan keçməlidir — ViewModel onları oxuyur.
        PlatformExtras.Current = new AndroidPlatformExtras(this);

        base.OnCreate(savedInstanceState);

        // Uzun konversiyalar üçün foreground xidməti işə salınır.
        NodeForegroundService.Start(this);
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
    }
}
