using Microsoft.AspNetCore.Mvc;

namespace Kela.Core.Modules.Subscription.Requests;

public class PlanRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    [FromForm(Name = "monthly_price")]
    public decimal MonthlyPrice { get; set; }

    public string Currency { get; set; } = "AZN";

    [FromForm(Name = "yearly_free_months")]
    public int YearlyFreeMonths { get; set; } = 2;

    [FromForm(Name = "max_teachers")]
    public int MaxTeachers { get; set; }

    [FromForm(Name = "max_students")]
    public int MaxStudents { get; set; }

    [FromForm(Name = "max_groups")]
    public int MaxGroups { get; set; }

    [FromForm(Name = "max_subjects")]
    public int MaxSubjects { get; set; }

    [FromForm(Name = "max_lessons")]
    public int MaxLessons { get; set; }

    [FromForm(Name = "monthly_video_minutes")]
    public int MonthlyVideoMinutes { get; set; }

    /// <summary>GB ilə daxil edilir, bayta çevrilir.</summary>
    [FromForm(Name = "storage_gb")]
    public int StorageGb { get; set; }

    [FromForm(Name = "concurrent_conversions")]
    public int ConcurrentConversions { get; set; } = 2;

    [FromForm(Name = "max_video_height")]
    public int MaxVideoHeight { get; set; } = 1080;

    [FromForm(Name = "addon_student_pack_size")]
    public int AddonStudentPackSize { get; set; } = 10;

    [FromForm(Name = "addon_student_pack_price")]
    public decimal AddonStudentPackPrice { get; set; }

    [FromForm(Name = "addon_video_pack_minutes")]
    public int AddonVideoPackMinutes { get; set; } = 600;

    [FromForm(Name = "addon_video_pack_price")]
    public decimal AddonVideoPackPrice { get; set; }

    [FromForm(Name = "addon_storage_pack_gb")]
    public int AddonStoragePackGb { get; set; } = 50;

    [FromForm(Name = "addon_storage_pack_price")]
    public decimal AddonStoragePackPrice { get; set; }

    [FromForm(Name = "max_addon_percent")]
    public int MaxAddonPercent { get; set; } = 100;

    [FromForm(Name = "original_retention_days")]
    public int OriginalRetentionDays { get; set; } = 30;

    /// <summary>Vergüllə ayrılmış açıqlar: certificates,payments,crm,chat,branding,api</summary>
    public string? Features { get; set; }

    [FromForm(Name = "is_active")]
    public bool IsActive { get; set; } = true;

    [FromForm(Name = "sort_order")]
    public int SortOrder { get; set; }
}

public class AssignPlanRequest
{
    public long PlanId { get; set; }

    [FromForm(Name = "expires_at")]
    public DateTimeOffset? ExpiresAt { get; set; }
}

public class AddAddonRequest
{
    /// <summary>Superadmin başqa tenant üçün əlavə edirsə. Boşdursa cari tenant.</summary>
    [FromForm(Name = "tenant_id")]
    public long? TenantId { get; set; }

    /// <summary>students | video | storage</summary>
    public string Kind { get; set; } = string.Empty;

    /// <summary>Neçə paket alınır.</summary>
    public int Packs { get; set; } = 1;

    public decimal? Amount { get; set; }

    public string? Note { get; set; }
}
