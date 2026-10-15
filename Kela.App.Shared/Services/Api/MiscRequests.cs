using Kela.App.Shared.Dtos;

namespace Kela.App.Shared.Services.Api;

public sealed partial class AppApi
{
    // --- Misc ---
    public Task<List<PrivacyPolicyDto>> PrivacyPolicyAsync() => _api.GetAsync<List<PrivacyPolicyDto>>("/api/privacy-policy");

    /// <summary>Login səhifəsi üçün aktiv tenant-lar (public).</summary>
    public Task<List<TenantDto>> TenantsAsync() => _api.GetAsync<List<TenantDto>>("/api/tenant");
}
