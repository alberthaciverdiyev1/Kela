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

    Task<BannerResponse> UpdateAsync(long id, BannerUpsertRequest request, CancellationToken ct = default);

    Task DeleteAsync(long id, CancellationToken ct = default);

    Task BulkAsync(BulkRequest request, CancellationToken ct = default);
}
