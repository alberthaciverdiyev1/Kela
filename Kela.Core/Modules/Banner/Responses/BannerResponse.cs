namespace Kela.Core.Modules.Banner.Responses;

public record BannerResponse(
    long Id,
    string Title,
    string Image,
    bool IsNew,
    bool IsActive = true,
    DateOnly? ActiveUntil = null);
