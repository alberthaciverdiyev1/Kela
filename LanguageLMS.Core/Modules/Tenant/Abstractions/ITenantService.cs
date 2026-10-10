using LanguageLMS.Core.Modules.Tenant.Requests;
using LanguageLMS.Core.Modules.Tenant.Responses;

namespace LanguageLMS.Core.Modules.Tenant.Abstractions;

public interface ITenantService
{
    Task<IReadOnlyList<TenantResponse>> ListAsync(CancellationToken ct = default);

    Task<TenantResponse> CreateAsync(CreateTenantRequest request, CancellationToken ct = default);
}
