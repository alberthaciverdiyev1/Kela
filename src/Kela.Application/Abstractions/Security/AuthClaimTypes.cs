namespace Kela.Application.Abstractions.Security;

/// <summary>
/// Cookie-only auth'ta kullanılan claim tip adları.
/// "tenant_id" login'de cookie'ye yazılır; TenantResolutionMiddleware okur.
/// </summary>
public static class AuthClaimTypes
{
    /// <summary>Kullanıcının bağlı olduğu tenant'ın Id'si.</summary>
    public const string TenantId = "tenant_id";
}
