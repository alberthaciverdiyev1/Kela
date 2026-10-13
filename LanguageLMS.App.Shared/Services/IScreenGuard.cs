namespace LanguageLMS.App.Shared.Services;

/// <summary>
/// Ekran qoruması qatı. Web-də sürtünmə + blur + watermark + aşkarlama (lms-guard.js),
/// native-də isə əlavə olaraq platforma bayraqları (Android FLAG_SECURE, iOS capture aşkarlama) tətbiq edilir.
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
