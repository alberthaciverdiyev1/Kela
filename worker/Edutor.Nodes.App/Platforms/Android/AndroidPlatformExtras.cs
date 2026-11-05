using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Edutor.Nodes.Worker.Device;

namespace Edutor.Nodes.App.Platforms.Android;

/// <summary>
/// Android-in arxa plan işini boğan məhdudiyyətlərini aradan qaldırır:
/// batareya optimizasiyası, bildiriş icazəsi və fayl girişi.
/// </summary>
public sealed class AndroidPlatformExtras(Context context) : IPlatformExtras
{
    private const int NotificationRequestCode = 4001;

    // Xidmət kontekstində UI yoxdur — icazə dialoqları yalnız activity ilə açılır.
    private readonly Activity? activity = context as Activity;

    public bool IsSupported => true;

    public IPlatformStatus? PowerStatus { get; } = new AndroidPlatformStatus(context);

    public string? NativeBinaryDirectory => context.ApplicationInfo?.NativeLibraryDir;

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
        if (activity is not { } ui) return;

        RequestNotifications(ui);
        await Task.Delay(400);

        RequestBatteryUnrestricted(ui);
        await Task.Delay(400);

        RequestStorage(ui);
    }

    private bool NotificationsAllowed()
        => Build.VERSION.SdkInt < BuildVersionCodes.Tiramisu
           || ContextCompat.CheckSelfPermission(context, global::Android.Manifest.Permission.PostNotifications)
              == Permission.Granted;

    private void RequestNotifications(Activity ui)
    {
        if (NotificationsAllowed()) return;

        ActivityCompat.RequestPermissions(ui,
            [global::Android.Manifest.Permission.PostNotifications], NotificationRequestCode);
    }

    /// <summary>Doze rejimi uzun konversiyaları dayandırır — istifadəçidən muafiyyət istənilir.</summary>
    private bool BatteryUnrestricted()
    {
        var manager = (PowerManager?)context.GetSystemService(Context.PowerService);

        return manager?.IsIgnoringBatteryOptimizations(context.PackageName!) ?? false;
    }

    private void RequestBatteryUnrestricted(Activity ui)
    {
        if (BatteryUnrestricted()) return;

        try
        {
            var intent = new Intent(Settings.ActionRequestIgnoreBatteryOptimizations);
            intent.SetData(global::Android.Net.Uri.Parse($"package:{context.PackageName}"));

            ui.StartActivity(intent);
        }
        catch (Exception ex) when (ex is ActivityNotFoundException or Java.Lang.SecurityException)
        {
            // Bəzi ROM-larda bu ekran yoxdur — ümumi batareya ayarlarına düşülür.
            ui.StartActivity(new Intent(Settings.ActionIgnoreBatteryOptimizationSettings));
        }
    }

    /// <summary>Android 11+ üçün "bütün fayllara giriş"; köhnə versiyalarda adi icazə.</summary>
    private bool StorageAllowed()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
        {
            return global::Android.OS.Environment.IsExternalStorageManager;
        }

        return ContextCompat.CheckSelfPermission(context, global::Android.Manifest.Permission.WriteExternalStorage)
               == Permission.Granted;
    }

    private void RequestStorage(Activity ui)
    {
        if (StorageAllowed()) return;

        try
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                var intent = new Intent(Settings.ActionManageAppAllFilesAccessPermission);
                intent.SetData(global::Android.Net.Uri.Parse($"package:{context.PackageName}"));

                ui.StartActivity(intent);
            }
            else
            {
                ActivityCompat.RequestPermissions(ui, [
                    global::Android.Manifest.Permission.ReadExternalStorage,
                    global::Android.Manifest.Permission.WriteExternalStorage
                ], NotificationRequestCode + 1);
            }
        }
        catch (ActivityNotFoundException)
        {
            ui.StartActivity(new Intent(Settings.ActionManageAllFilesAccessPermission));
        }
    }
}
