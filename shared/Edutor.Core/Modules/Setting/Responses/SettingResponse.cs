namespace Edutor.Core.Modules.Setting.Responses;

public record SettingResponse(
    string? Name,
    long Id,
    string? Logo,
    string? Description,
    string? Email,
    string? Address,
    string? WhatsappNumber,
    string? ContactNumber1,
    string? ContactNumber2,
    string? ContactNumber3,
    string? ContactNumber4,
    string? InstagramUrl,
    string? FacebookUrl,
    string? AndroidMinVersion,
    string? IosMinVersion);

public record AppVersionPlatform(string? MinVersion, string? StoreUrl);

public record AppVersionResponse(AppVersionPlatform Android, AppVersionPlatform Ios);
