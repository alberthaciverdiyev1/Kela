namespace LanguageLMS.Core.Modules.Setting.Responses;

public record SettingResponse(
    long Id,
    string? Logo,
    string? WhatsappNumber,
    string? ContactNumber1,
    string? ContactNumber2,
    string? ContactNumber3,
    string? ContactNumber4,
    string? InstagramUrl,
    string? FacebookUrl);

public record AppVersionPlatform(string? MinVersion, string? StoreUrl);

public record AppVersionResponse(AppVersionPlatform Android, AppVersionPlatform Ios);
