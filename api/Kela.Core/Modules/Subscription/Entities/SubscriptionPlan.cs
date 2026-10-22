namespace Kela.Core.Modules.Subscription.Entities;

/// <summary>
/// Platforma səviyyəsində abunəlik planı. Tenant-a bağlı DEYİL — bütün platforma üçün
/// ortaqdır, ona görə <c>BaseEntity</c> deyil (tenant filtri tətbiq olunmasın).
/// Bütün qiymət və limitlər admin paneldən dəyişdirilir.
/// </summary>
public class SubscriptionPlan
{
    public long Id { get; set; }

    /// <summary>Dəyişməz kod: basic | pro | premium | advanced.</summary>
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal MonthlyPrice { get; set; }

    public string Currency { get; set; } = "AZN";

    /// <summary>İllik ödənişdə pulsuz ayların sayı (məs. 2 = 10 ay qiyməti).</summary>
    public int YearlyFreeMonths { get; set; } = 2;

    // ---- Limitlər (0 və ya mənfi = limitsiz) ----
    public int MaxTeachers { get; set; }
    public int MaxStudents { get; set; }
    public int MaxGroups { get; set; }
    public int MaxSubjects { get; set; }
    public int MaxLessons { get; set; }

    /// <summary>Aylıq video konversiya limiti (mənbə dəqiqəsi).</summary>
    public int MonthlyVideoMinutes { get; set; }

    public long StorageBytes { get; set; }

    /// <summary>Bir tenant üçün eyni vaxtda icra olunan konversiya sayı.</summary>
    public int ConcurrentConversions { get; set; }

    /// <summary>Ən yüksək çıxış hündürlüyü: 720 və ya 1080.</summary>
    public int MaxVideoHeight { get; set; } = 1080;

    // ---- Əlavə limit qiymətləri (istənilən sayda almaq olar) ----
    /// <summary>Bir əlavə tələbə yerinin qiyməti.</summary>
    public decimal PricePerStudent { get; set; }

    /// <summary>Bir əlavə qrupun qiyməti.</summary>
    public decimal PricePerGroup { get; set; }

    /// <summary>Bir əlavə video saatının qiyməti.</summary>
    public decimal PricePerVideoHour { get; set; }

    // ---- Digər ----
    /// <summary>Orijinal videonun neçə gün saxlanacağı (0 = limitsiz).</summary>
    public int OriginalRetentionDays { get; set; } = 30;

    /// <summary>JSON: {"certificates":true,"payments":true,"crm":false,"chat":false,"branding":false,"api":false}</summary>
    public string Features { get; set; } = "{}";

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; set; }
}
