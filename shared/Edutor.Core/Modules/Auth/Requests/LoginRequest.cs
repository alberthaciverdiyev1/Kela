namespace Edutor.Core.Modules.Auth.Requests;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    /// <summary>Login ediləcək tenant. Verilməzsə, X-Tenant-Id header-ı istifadə olunur.</summary>
    public long? TenantId { get; set; }

    /// <summary>Verilərsə, hesab bu cihaza bağlanır (device lock).</summary>
    public string? DeviceId { get; set; }
}
