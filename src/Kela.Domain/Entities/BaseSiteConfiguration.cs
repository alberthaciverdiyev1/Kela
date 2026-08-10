namespace Kela.Domain.Entities;

public sealed class BaseSiteConfiguration
{
    public const int SingletonId = 1;

    public int Id { get; set; }

    // ── Genel ──
    public string SiteName { get; private set; } = "Kela LMS";

    // ── Tema renkleri (base hex — 500 kademesi; ön yüz 50–900'ü türetir) ──
    public string PrimaryColor { get; private set; } = "#4f46e5";
    public string SecondaryColor { get; private set; } = "#64748b";
    public string SuccessColor { get; private set; } = "#22c55e";
    public string WarningColor { get; private set; } = "#f59e0b";
    public string ErrorColor { get; private set; } = "#ef4444";
    public string InfoColor { get; private set; } = "#0ea5e9";

    // ── Arayüz ──
    public string NavMode { get; private set; } = "navbar";

    // ── Bildirim Sistemi ──
    public string NotificationProvider { get; private set; } = "sweetalert";

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public void Update(
        string siteName,
        string primary, string secondary, string success, string warning, string error, string info,
        string navMode,
        string notificationProvider)
    {
        SiteName = siteName;
        PrimaryColor = primary;
        SecondaryColor = secondary;
        SuccessColor = success;
        WarningColor = warning;
        ErrorColor = error;
        InfoColor = info;
        NavMode = navMode;
        NotificationProvider = notificationProvider;
        UpdatedAt = DateTime.UtcNow;
    }
}
