namespace Kela.Core.Modules.Auth.Requests;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    /// <summary>Verilərsə, hesab bu cihaza bağlanır (device lock).</summary>
    public string? DeviceId { get; set; }
}
