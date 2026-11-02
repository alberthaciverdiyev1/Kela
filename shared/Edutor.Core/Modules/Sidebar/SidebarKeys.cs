namespace Edutor.Core.Modules.Sidebar;

/// <summary>Sidebar menyu açarları — hesablamalar, etiketlər və etiket idarəetməsi bu siyahıya söykənir.</summary>
public static class SidebarKeys
{
    public sealed record Item(string Key, string Name, string? LabelKey = null);

    public static readonly Item[] All =
    [
        new("dashboard", "Dashboard", "Nav_Dashboard"),
        new("subject", "Fənlər", "Nav_Subjects"),
        new("lesson", "Dərslər", "Nav_Lessons"),
        new("lesson-content", "Məzmun", "Nav_Materials"),
        new("question", "Suallar", "Nav_Questions"),
        new("quiz", "İmtahanlar", "Nav_Exams"),
        new("homework", "Tapşırıqlar", "Nav_Homework"),
        new("attendance", "Davamiyyət", "Nav_Attendance"),
        new("calendar", "Təqvim", "Nav_Calendar"),
        new("teacher-note", "Qeydlər", "Nav_Notes"),
        new("group", "Qruplar", "Nav_Groups"),
        new("group-request", "Qrup müraciətləri"),
        new("group-stat", "Qrup statistikası", "Nav_GroupStats"),
        new("students", "Tələbələr", "Nav_Students"),
        new("platform-finance", "Platform gəliri", "Nav_PlatformFinance"),
        new("subscription", "Limitlər"),
        new("plan-catalog", "Planlar"),
        new("addon-request", "Limit müraciətləri"),
        new("plan", "Plan idarəetməsi"),
        new("payment", "Ödənişlər"),
        new("payment-plan", "Plan şablonları"),
        new("cashbox", "Kassa"),
        new("banner", "Bannerlər", "Nav_Banners"),
        new("privacy-policy", "Məxfilik siyasəti", "Nav_Privacy"),
        new("notification", "Bildirişlər", "Nav_Notifications"),
        new("certificate", "Sertifikatlar", "Nav_Certificates"),
        new("user", "İstifadəçilər", "Nav_Users"),
        new("setting", "Parametrlər", "Nav_Settings"),
        new("config", "Konfiqurasiya", "Nav_Config"),
        new("platform-setting", "Platforma parametrləri", "Nav_PlatformSettings"),
        new("sidebar-label", "Menyu etiketləri"),
        new("tenant", "Tenantlar", "Nav_Tenants"),
        new("node", "Node-lar", "Nav_Nodes"),
        new("feedback", "Geribildiriş", "Feedback_Title")
    ];

    public static Item? Find(string key) => All.FirstOrDefault(x => x.Key == key);
}
