namespace Kela.Application.Features.SiteConfiguration.Responses;

public sealed record SiteConfigurationResponse(
    string SiteName,
    string PrimaryColor,
    string SecondaryColor,
    string SuccessColor,
    string WarningColor,
    string ErrorColor,
    string InfoColor,
    string NavMode,
    string NotificationProvider);
