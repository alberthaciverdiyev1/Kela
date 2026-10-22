namespace Kela.Core.Modules.Tenant.Entities;

public class Tenant
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    /// <summary>Özünəqeydiyyatdan gəlib superadmin təsdiqini gözləyir.</summary>
    public bool IsPending { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // ---- Abunəlik ----
    /// <summary>Seçilmiş plan (subscription_plans.id). Null = default plan.</summary>
    public long? PlanId { get; set; }

    public DateTimeOffset? PlanStartedAt { get; set; }

    /// <summary>Planın bitmə tarixi. Null = müddətsiz.</summary>
    public DateTimeOffset? PlanExpiresAt { get; set; }
}
