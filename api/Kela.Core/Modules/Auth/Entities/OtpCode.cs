namespace Kela.Core.Modules.Auth.Entities;

public class OtpCode
{
    public long Id { get; set; }

    public long TenantId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? UsedAt { get; set; }

    /// <summary>Səhv cəhdlərin sayı — brute-force qarşısı üçün.</summary>
    public int Attempts { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
