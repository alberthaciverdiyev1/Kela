using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Kela.Nodes.Worker.Device;

namespace Kela.Nodes.App.Platforms.Android;

/// <summary>
/// Android-in arxa plan işini boğan məhdudiyyətlərini aradan qaldırır:
/// batareya optimizasiyası, bildiriş icazəsi və fayl girişi.
/// </summary>
public sealed class AndroidPlatformExtras(Activity activity) : IPlatformExtras
{
    private const int NotificationRequestCode = 4001;

    public bool IsSupported => true;

    public IPlatformStatus? PowerStatus { get; } = new AndroidPlatformStatus(activity);

    public string? NativeBinaryDirectory => activity.ApplicationInfo?.NativeLibraryDir;

    public string Status()
    {
        var parts = new List<string>
        {
            NotificationsAllowed() ? "Bildirişlər: açıq" : "Bildirişlər: bağlı",
            BatteryUnrestricted() ? "Batareya muafiyyəti: var" : "Batareya muafiyyəti: yoxdur",
            StorageAllowed() ? "Fayl girişi: var" : "Fayl girişi: yoxdur"
        };

        return string.Join("  ·  ", parts);
    }

    public async Task RequestAllAsync()
    {
        RequestNotifications();
        await Task.Delay(400);

        RequestBatteryUnrestricted();
        await Task.Delay(400);

        RequestStorage();
    }

    private bool NotificationsAllowed()
        => Build.VERSION.SdkInt < BuildVersionCodes.Tiramisu
           || ContextCompat.CheckSelfPermission(activity, global::Android.Manifest.Permission.PostNotifications)
              == Permission.Granted;

    private void RequestNotifications()
    {
        if (NotificationsAllowed()) return;

        ActivityCompat.RequestPermissions(activity,
            [global::Android.Manifest.Permission.PostNotifications], NotificationRequestCode);
    }

    /// <summary>Doze rejimi uzun konversiyaları dayandırır — istifadəçidən muafiyyət istənilir.</summary>
    private bool BatteryUnrestricted()
    {
        var manager = (PowerManager?)activity.GetSystemService(Context.PowerService);

        return manager?.IsIgnoringBatteryOptimizations(activity.PackageName!) ?? false;
    }

    private void RequestBatteryUnrestricted()
    {
        if (BatteryUnrestricted()) return;

        try
        {
            var intent = new Intent(Settings.ActionRequestIgnoreBatteryOptimizations);
            intent.SetData(global::Android.Net.Uri.Parse($"package:{activity.PackageName}"));

            activity.StartActivity(intent);
        }
        catch (Exception ex) when (ex is ActivityNotFoundException or Java.Lang.SecurityException)
        {
            // Bəzi ROM-larda bu ekran yoxdur — ümumi batareya ayarlarına düşülür.
            activity.StartActivity(new Intent(Settings.ActionIgnoreBatteryOptimizationSettings));
        }
    }

    /// <summary>Android 11+ üçün "bütün fayllara giriş"; köhnə versiyalarda adi icazə.</summary>
    private bool StorageAllowed()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
        {
            return global::Android.OS.Environment.IsExternalStorageManager;
        }

        return ContextCompat.CheckSelfPermission(activity, global::Android.Manifest.Permission.WriteExternalStorage)
               == Permission.Granted;
    }

    private void RequestStorage()
    {
        if (StorageAllowed()) return;

        try
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                var intent = new Intent(Settings.ActionManageAppAllFilesAccessPermission);
                intent.SetData(global::Android.Net.Uri.Parse($"package:{activity.PackageName}"));

                activity.StartActivity(intent);
            }
            else
            {
                ActivityCompat.RequestPermissions(activity, [
                    global::Android.Manifest.Permission.ReadExternalStorage,
                    global::Android.Manifest.Permission.WriteExternalStorage
                ], NotificationRequestCode + 1);
            }
        }
        catch (ActivityNotFoundException)
        {
            activity.StartActivity(new Intent(Settings.ActionManageAllFilesAccessPermission));
        }
    }
}
