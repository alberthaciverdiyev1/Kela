#if ANDROID
using Android.Views;
using Microsoft.Maui.ApplicationModel;
#endif
#if IOS
using Foundation;
using UIKit;
#endif

namespace Kela.App.Shared.Services;
public static partial class NativeScreenGuard
{
    public static bool IsCaptured { get; private set; }

    public static void Apply()
    {
        #if ANDROID
                var activity = Platform.CurrentActivity;
                activity?.Window?.SetFlags(WindowManagerFlags.Secure, WindowManagerFlags.Secure);
        #elif IOS
                UIScreen.Notifications.ObserveCapturedDidChange(OnCapturedChanged);
                OnCapturedChanged(null, null);
        #endif
    }

    public static void Remove()
    {
        #if ANDROID
                var activity = Platform.CurrentActivity;
                activity?.Window?.ClearFlags(WindowManagerFlags.Secure);
        #endif
            }

        #if IOS
            private static void OnCapturedChanged(object? sender, NSNotificationEventArgs? e)
                => IsCaptured = UIScreen.MainScreen.Captured;
        #endif
}