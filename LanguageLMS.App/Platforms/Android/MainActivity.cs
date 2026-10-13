using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace LanguageLMS.App;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Native ekran qoruması: screenshot + ekran yazılışını bloklayır (web-də mümkün deyil).
        Window?.SetFlags(WindowManagerFlags.Secure, WindowManagerFlags.Secure);
    }
}
