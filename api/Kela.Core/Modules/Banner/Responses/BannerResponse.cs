namespace Kela.Core.Modules.Banner.Responses;

public record BannerResponse(
    long Id,
    string Title,
    string Image,
    bool IsNew,
    bool IsActive = true,
    DateOnly? ActiveUntil = null,
    /// <summary>Bannerin sahibi. Null — admin tərəfindən yaradılıb.</summary>
    long? UserId = null,
    string? OwnerName = null);
