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
}
