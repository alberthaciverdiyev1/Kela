using Kela.Core.Modules.Tenant.Requests;
using Kela.Core.Modules.Tenant.Responses;

namespace Kela.Core.Modules.Tenant.Abstractions;

public interface ITenantService
{
    Task<IReadOnlyList<TenantResponse>> ListAsync(CancellationToken ct = default);

    /// <summary>Login səhifəsi üçün yalnız aktiv tenant-lar (public).</summary>
    Task<IReadOnlyList<TenantResponse>> ListPublicAsync(CancellationToken ct = default);

    Task<TenantResponse?> GetByIdAsync(long id, CancellationToken ct = default);

    Task<TenantResponse> CreateAsync(CreateTenantRequest request, CancellationToken ct = default);
}
