namespace Kela.Application.Features.SiteConfiguration.Requests;

/// <summary>
/// TEK istek: tüm site konfigürasyonunu günceller (Admin/Teacher).
/// Yeni bir ayar eklediğinde buraya bir alan ekle.
/// </summary>
public sealed record UpdateSiteConfigurationRequest(
    string SiteName,
    string PrimaryColor,
    string SecondaryColor,
    string SuccessColor,
    string WarningColor,
    string ErrorColor,
    string InfoColor,
    string NavMode);
