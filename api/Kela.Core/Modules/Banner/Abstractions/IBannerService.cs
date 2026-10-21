using Kela.Core.Common.Bulk;
using Kela.Core.Common.Responses;
using Kela.Core.Modules.Banner.Requests;
using Kela.Core.Modules.Banner.Responses;

namespace Kela.Core.Modules.Banner.Abstractions;

public interface IBannerService
{
    Task<PagedResponse<BannerResponse>> ListAsync(BannerListRequest request, CancellationToken ct = default);

    Task<BannerResponse> GetAsync(long id, CancellationToken ct = default);

    Task<BannerResponse> CreateAsync(BannerUpsertRequest request, CancellationToken ct = default);

    /// <summary><paramref name="ownerId"/> verilərsə yalnız həmin sahibin bannerinə icazə verilir.</summary>
    Task<BannerResponse> UpdateAsync(long id, BannerUpsertRequest request, long? ownerId = null, CancellationToken ct = default);

    Task DeleteAsync(long id, long? ownerId = null, CancellationToken ct = default);

    Task SetActiveAsync(long id, bool isActive, long? ownerId = null, CancellationToken ct = default);

    Task BulkAsync(BulkRequest request, long? ownerId = null, CancellationToken ct = default);
}
