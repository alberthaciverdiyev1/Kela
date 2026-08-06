namespace Kela.Domain.Entities;

/// <summary>
/// SİTE GENELİ TEK KONFİGÜRASYON — singleton (her zaman tek satır, Id = 1).
///
/// Tüm site ayarları burada toplu durur. Yeni bir ayar eklemek için:
///   1) Buraya bir property ekle (varsayılan değerini de ver)  ← "neler olabileceği orda"
///   2) EF Configuration + migration
///   3) Response/Request'e aynı alanı ekle
///   4) Ayarlar sayfasına ilgili kart
/// Yeni ayar otomatik olarak tek GET/PUT ile TÜM kullanıcılara yayılır.
/// Varsayılanlar bu entity'deki property ilk değerleridir; satır yoksa servis
/// varsayılanlarla oluşturur.
/// </summary>
public sealed class BaseSiteConfiguration
{
    /// <summary>Singleton satırın sabit Id'si.</summary>
    public const int SingletonId = 1;

    public int Id { get; set; }

    // ── Genel ──
    /// <summary>Site adı (navbar/sidebar/oturum ekranlarında görünür).</summary>
    public string SiteName { get; private set; } = "Kela LMS";

    // ── Tema renkleri (base hex — 500 kademesi; ön yüz 50–900'ü türetir) ──
    public string PrimaryColor { get; private set; } = "#4f46e5";
    public string SecondaryColor { get; private set; } = "#64748b";
    public string SuccessColor { get; private set; } = "#22c55e";
    public string WarningColor { get; private set; } = "#f59e0b";
    public string ErrorColor { get; private set; } = "#ef4444";
    public string InfoColor { get; private set; } = "#0ea5e9";

    // ── Arayüz ──
    /// <summary>Site geneli düzen: "navbar" | "sidebar".</summary>
    public string NavMode { get; private set; } = "navbar";

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public void Update(
        string siteName,
        string primary, string secondary, string success, string warning, string error, string info,
        string navMode)
    {
        SiteName = siteName;
        PrimaryColor = primary;
        SecondaryColor = secondary;
        SuccessColor = success;
        WarningColor = warning;
        ErrorColor = error;
        InfoColor = info;
        NavMode = navMode;
        UpdatedAt = DateTime.UtcNow;
    }
}
