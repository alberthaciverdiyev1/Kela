using LanguageLMS.Core.Common.Responses;
using LanguageLMS.Core.Modules.Banner.Requests;
using LanguageLMS.Core.Modules.Banner.Responses;

namespace LanguageLMS.Core.Modules.Banner.Services;

public interface IBannerService
{
    Task<PagedResponse<BannerResponse>> ListAsync(BannerListRequest request, CancellationToken ct = default);

    Task<BannerResponse> GetAsync(long id, CancellationToken ct = default);

    Task<BannerResponse> CreateAsync(BannerUpsertRequest request, CancellationToken ct = default);

    Task<BannerResponse> UpdateAsync(long id, BannerUpsertRequest request, CancellationToken ct = default);

    Task DeleteAsync(long id, CancellationToken ct = default);
}
