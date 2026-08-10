namespace Kela.Application.Features.SiteConfiguration.Requests;

public sealed record UpdateSiteConfigurationRequest(
    string SiteName,
    string PrimaryColor,
    string SecondaryColor,
    string SuccessColor,
    string WarningColor,
    string ErrorColor,
    string InfoColor,
    string NavMode,
    string NotificationProvider);
