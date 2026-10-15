namespace Kela.Core.Modules.Auth.Requests;

public class RegisterRequest
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string Password { get; set; } = string.Empty;

    /// <summary>Qeydiyyatdan keçiləcək tenant. Verilməzsə, X-Tenant-Id header-ı istifadə olunur.</summary>
    public long? TenantId { get; set; }
}
