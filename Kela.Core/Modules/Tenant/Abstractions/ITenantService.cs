using Kela.Core.Modules.Tenant.Requests;
using Kela.Core.Modules.Tenant.Responses;

namespace Kela.Core.Modules.Tenant.Abstractions;

public interface ITenantService
{
    Task<IReadOnlyList<TenantResponse>> ListAsync(CancellationToken ct = default);

    Task<TenantResponse> CreateAsync(CreateTenantRequest request, CancellationToken ct = default);
}
