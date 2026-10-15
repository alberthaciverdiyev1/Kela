using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.Activity;
using AndroidX.Core.View;

namespace Kela.App;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Screenshot və ekran yazılışını bloklayır — WebView-da (lms-guard.js) mümkün deyil.
        Window?.SetFlags(WindowManagerFlags.Secure, WindowManagerFlags.Secure);

        // Status bar ikonları (saat, batareya, şəbəkə) tünd olmalıdır: tətbiqin fonu
        // açıqdır və ağ ikonlar ağ fon üzərində oxunmur.
        if (Window is not null)
        {
            WindowCompat.GetInsetsController(Window, Window.DecorView).AppearanceLightStatusBars = true;
        }

        // WebView-da geri tuşu default olaraq aktiviti bağlayır; bu callback onu
        // Blazor naviqasiya yığınına bağlayır (OnBackPressed API 33-də deprecated-dir).
        OnBackPressedDispatcher.AddCallback(this, new BackPressCallback(this));
    }

    private Android.Webkit.WebView? FindWebView() => Find(Window?.DecorView);

    private static Android.Webkit.WebView? Find(Android.Views.View? view)
    {
        switch (view)
        {
            case Android.Webkit.WebView webView:
                return webView;

            case Android.Views.ViewGroup group:
                for (var i = 0; i < group.ChildCount; i++)
                {
                    if (Find(group.GetChildAt(i)) is { } found) return found;
                }
                break;
        }

        return null;
    }

    private sealed class BackPressCallback(MainActivity activity) : OnBackPressedCallback(true)
    {
        public override void HandleOnBackPressed()
        {
            var webView = activity.FindWebView();
            if (webView is null)
            {
                activity.Finish();
                return;
            }

            webView.EvaluateJavascript(
                "window.kelaNative ? window.kelaNative.handleBack() : false",
                new BackResultCallback(activity));
        }
    }

    private sealed class BackResultCallback(MainActivity activity) : Java.Lang.Object, Android.Webkit.IValueCallback
    {
        // JS tərəf geri gedə bilmirsə yığın boşdur — tətbiq bağlanır (native davranış).
        public void OnReceiveValue(Java.Lang.Object? value)
        {
            if (value?.ToString()?.Trim('"') != "true") activity.Finish();
        }
    }
}
