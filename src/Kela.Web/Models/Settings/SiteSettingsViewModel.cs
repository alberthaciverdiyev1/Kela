namespace Kela.Web.Models.Settings;

public sealed class SiteSettingsViewModel
{
    public string SiteName { get; set; } = "Kela";
    public string NavMode { get; set; } = "navbar";
    public string NotificationProvider { get; set; } = "sweetalert";
    public string PrimaryColor { get; set; } = "#4f46e5";
    public string SecondaryColor { get; set; } = "#64748b";
    public string SuccessColor { get; set; } = "#22c55e";
    public string WarningColor { get; set; } = "#f59e0b";
    public string ErrorColor { get; set; } = "#ef4444";
    public string InfoColor { get; set; } = "#0ea5e9";
}
