namespace Edutor.Core.Modules.User.Requests;

public class UserLoginRequest
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    /// <summary>Login ediləcək tenant. Superadmin üçün boş buraxıla bilər.</summary>
    public long? TenantId { get; set; }
}
