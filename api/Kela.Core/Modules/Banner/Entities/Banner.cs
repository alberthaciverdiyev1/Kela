using Kela.Core.Common.Entities;

namespace Kela.Core.Modules.Banner.Entities;

public class Banner : BaseEntity, IActivatable
{
    /// <summary>Banneri yaradan istifadəçi. Null — admin tərəfindən yaradılıb.</summary>
    public long? UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Image { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>Banner nə vaxta qədər göstərilsin (null = müddətsiz).</summary>
    public DateOnly? ActiveUntil { get; set; }

    /// <summary>Kimə görünür — <see cref="BannerScope"/>.</summary>
    public string Scope { get; set; } = BannerScope.Tenant;
}

public static class BannerScope
{
    /// <summary>Müəllim paylaşıb — yalnız onun qruplarındakı şagirdlər görür.</summary>
    public const string Teacher = "teacher";

    /// <summary>Admin paylaşıb — bütün tenant.</summary>
    public const string Tenant = "tenant";

    /// <summary>Superadmin paylaşıb — bütün tenantlar.</summary>
    public const string Global = "global";
}
