using Android.App;
using Android.Content;
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
    private string? _pendingDeepLink;
    private int _deepLinkAttempts;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        HandleDeepLink(Intent);

        // Screenshot və ekran yazılışını bloklayır — WebView-da (lms-guard.js) mümkün deyil.
        Window?.SetFlags(WindowManagerFlags.Secure, WindowManagerFlags.Secure);

        // Status bar ikonları (saat, batareya, şəbəkə) tünd olmalıdır: tətbiqin fonu
        // açıqdır və ağ ikonlar ağ fon üzərində oxunmur.
        var window = Window;
        var decorView = window?.DecorView;
        var insets = window is not null && decorView is not null
            ? WindowCompat.GetInsetsController(window, decorView)
            : null;

        if (insets is not null)
        {
            insets.AppearanceLightStatusBars = true;
        }

        // WebView-da geri tuşu default olaraq aktiviti bağlayır; bu callback onu
        // Blazor naviqasiya yığınına bağlayır (OnBackPressed API 33-də deprecated-dir).
        OnBackPressedDispatcher.AddCallback(this, new BackPressCallback(this));
    }

    /// <summary>https://host/join/{token} və ya kela://join/{token} linkini tətbiq daxilində açır.</summary>
    private void HandleDeepLink(Intent? intent)
    {
        var data = intent?.Data;
        if (data is null) return;

        string? path = data.Scheme == "kela"
            ? $"/{data.Host}{data.Path}"
            : data.Scheme == "https" ? data.Path : null;

        if (path is null || !path.StartsWith("/join/", StringComparison.OrdinalIgnoreCase)) return;

        _pendingDeepLink = path;
        _deepLinkAttempts = 0;
        TryNavigateDeepLink();
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        HandleDeepLink(intent);
    }

    private void TryNavigateDeepLink()
    {
        if (_pendingDeepLink is not { } path) return;

        var webView = FindWebView();
        if (webView is null)
        {
            ScheduleDeepLinkRetry();
            return;
        }

        // JS körpüsü Blazor hazır olanda qeydiyyatdan keçir; hazır deyilsə təkrar yoxlayırıq.
        var jsonPath = System.Text.Json.JsonSerializer.Serialize(path);
        webView.EvaluateJavascript(
            $"typeof window.kelaNavigate === 'function' ? (window.kelaNavigate({jsonPath}), 'true') : 'false'",
            new DeepLinkResultCallback(this));
    }

    private void ScheduleDeepLinkRetry()
    {
        if (_pendingDeepLink is null || _deepLinkAttempts++ >= 20)
        {
            _pendingDeepLink = null;
            return;
        }

        new Handler(Looper.MainLooper!).PostDelayed(new Java.Lang.Runnable(TryNavigateDeepLink), 500);
    }

    private sealed class DeepLinkResultCallback(MainActivity activity) : Java.Lang.Object, Android.Webkit.IValueCallback
    {
        public void OnReceiveValue(Java.Lang.Object? value)
        {
            if (value?.ToString()?.Trim('"') == "true")
            {
                activity._pendingDeepLink = null;
                return;
            }

            activity.ScheduleDeepLinkRetry();
        }
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
