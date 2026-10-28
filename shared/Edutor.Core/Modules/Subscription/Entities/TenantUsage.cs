using Edutor.Core.Common.Entities;

namespace Edutor.Core.Modules.Subscription.Entities;

/// <summary>Tenant-ın bir ay üzrə istifadəsi və həmin ay alınmış əlavə paketlər.</summary>
public class TenantUsage : BaseEntity
{
    /// <summary>"2026-10" formatında dövr.</summary>
    public string Period { get; set; } = string.Empty;

    /// <summary>Uğurlu konversiyalardan toplanan mənbə dəqiqəsi.</summary>
    public int VideoMinutes { get; set; }

    public long StorageBytes { get; set; }

    public int AddonStudents { get; set; }

    public int AddonVideoMinutes { get; set; }

    public int AddonGroups { get; set; }
}
