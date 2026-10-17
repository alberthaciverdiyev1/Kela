using Android.Content;
using Android.OS;
using Android.OS.Storage;
using Kela.Nodes.Worker.Device;

namespace Kela.Nodes.App.Platforms.Android;

/// <summary>
/// Batareya və istilik göstəriciləri. Temperatur yalnız bəzi cihazlarda və
/// yalnız <c>BatteryManager.EXTRA_TEMPERATURE</c> vasitəsilə əlçatandır — ölçmə
/// mümkün deyilsə <c>null</c> qaytarılır və limit tətbiq olunmur.
/// </summary>
public sealed class AndroidPlatformStatus(Context context) : IPlatformStatus
{
    public int? BatteryPercent
    {
        get
        {
            var manager = (BatteryManager?)context.GetSystemService(Context.BatteryService);
            var level = manager?.GetIntProperty((int)BatteryProperty.Capacity) ?? -1;

            return level is >= 0 and <= 100 ? level : null;
        }
    }

    public bool? IsCharging
    {
        get
        {
            var manager = (BatteryManager?)context.GetSystemService(Context.BatteryService);
            var status = manager?.GetIntProperty((int)BatteryProperty.Status) ?? -1;

            return status switch
            {
                (int)BatteryStatus.Charging or (int)BatteryStatus.Full => true,
                (int)BatteryStatus.Discharging or (int)BatteryStatus.NotCharging => false,
                _ => null
            };
        }
    }

    public double? TemperatureCelsius
    {
        get
        {
            var intent = context.RegisterReceiver(null, new IntentFilter(Intent.ActionBatteryChanged));
            var tenths = intent?.GetIntExtra(BatteryManager.ExtraTemperature, int.MinValue) ?? int.MinValue;

            return tenths == int.MinValue ? null : tenths / 10.0;
        }
    }

    /// <summary>Android-də batareya və istilik ölçülə bilir, ona görə limitlər tətbiq olunur.</summary>
    public bool SupportsPowerLimits => true;

    /// <summary>
    /// Tətbiqin məlumat bölməsindəki boş yer. <c>DriveInfo("/")</c> burada sistem
    /// bölməsini göstərdiyi üçün StatFs ilə düzgün bölmə ölçülür.
    /// </summary>
    public int? FreeDiskMb
    {
        get
        {
            try
            {
                var path = global::Android.OS.Environment.DataDirectory?.AbsolutePath;
                if (string.IsNullOrEmpty(path)) return null;

                var stat = new StatFs(path);

                return (int)Math.Clamp(stat.AvailableBytes / (1024 * 1024), 0, int.MaxValue);
            }
            catch (Exception ex) when (ex is Java.Lang.IllegalArgumentException or Java.Lang.SecurityException)
            {
                return null;
            }
        }
    }
}
