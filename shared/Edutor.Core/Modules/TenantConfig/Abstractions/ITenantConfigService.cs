using Edutor.Core.Modules.TenantConfig.Requests;
using Edutor.Core.Modules.TenantConfig.Responses;

namespace Edutor.Core.Modules.TenantConfig.Abstractions;

public interface ITenantConfigService
{
    Task<TenantConfigResponse> GetAsync(CancellationToken ct = default);

    Task<TenantConfigResponse> UpdateAsync(TenantConfigUpsertRequest request, CancellationToken ct = default);
}
