namespace Edutor.Core.Modules.Banner.Responses;

public record BannerResponse(
    long Id,
    string Title,
    string Image,
    bool IsNew,
    bool IsActive = true,
    DateOnly? ActiveUntil = null,
    /// <summary>Bannerin sahibi. Null — admin tərəfindən yaradılıb.</summary>
    long? UserId = null,
    string? OwnerName = null,
    /// <summary>Bannerin aid olduğu tenant (kurs). Superadmin hansı kursun olduğunu görsün deyə.</summary>
    long TenantId = 0,
    string? TenantName = null,
    string Scope = "tenant");
