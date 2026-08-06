namespace Kela.Application.Features.SiteConfiguration.Responses;

/// <summary>
/// TEK yanıt: site geneli tüm ayarlar. Yeni bir ayar eklediğinde
/// buraya bir alan ekle → tüm kullanıcılar otomatik görür.
/// </summary>
public sealed record SiteConfigurationResponse(
    string SiteName,
    string PrimaryColor,
    string SecondaryColor,
    string SuccessColor,
    string WarningColor,
    string ErrorColor,
    string InfoColor,
    string NavMode);
