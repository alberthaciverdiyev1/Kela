namespace Kela.App.Shared.Services;

/// <summary>
/// Ekran qoruması qatı: sürtünmə + blur + watermark + aşkarlama (lms-guard.js).
/// OS səviyyəsində blok (Android FLAG_SECURE) platforma host-unda tətbiq olunur — MainActivity.OnCreate.
/// </summary>
public interface IScreenGuard
{
    /// <summary>Qorumasını aktivləşdirir və istifadəçiyə aid watermark mətnini təyin edir.</summary>
    ValueTask EnableAsync(string? watermark = null);

    /// <summary>Yalnız watermark-ı silir; sürtünmə/blur aktiv qalır (məs. çıxış zamanı).</summary>
    ValueTask ClearWatermarkAsync();

    /// <summary>Bütün qorumasını söndürür.</summary>
    ValueTask DisableAsync();
}
