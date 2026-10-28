using Edutor.Core.Common.Bulk;
using Edutor.Core.Common.Responses;
using Edutor.Core.Modules.Banner.Requests;
using Edutor.Core.Modules.Banner.Responses;

namespace Edutor.Core.Modules.Banner.Abstractions;

public interface IBannerService
{
    Task<PagedResponse<BannerResponse>> ListAsync(BannerListRequest request, CancellationToken ct = default);

    /// <summary>Şagirdin gördüyü story-lər: superadmin → bütün tenantlar, admin → cari tenant, müəllim → yalnız öz qrupları.</summary>
    Task<PagedResponse<BannerResponse>> ListForStudentAsync(long studentId, BannerListRequest request, CancellationToken ct = default);

    Task<BannerResponse> GetAsync(long id, CancellationToken ct = default);

    /// <summary>Şagirdin görə biləcəyi tək story — görünmürsə <see cref="KeyNotFoundException"/>.</summary>
    Task<BannerResponse> GetForStudentAsync(long studentId, long id, CancellationToken ct = default);

    Task<BannerResponse> CreateAsync(BannerUpsertRequest request, CancellationToken ct = default);

    /// <summary><paramref name="ownerId"/> verilərsə yalnız həmin sahibin bannerinə icazə verilir.</summary>
    Task<BannerResponse> UpdateAsync(long id, BannerUpsertRequest request, long? ownerId = null, CancellationToken ct = default);

    Task DeleteAsync(long id, long? ownerId = null, CancellationToken ct = default);

    Task SetActiveAsync(long id, bool isActive, long? ownerId = null, CancellationToken ct = default);

    Task BulkAsync(BulkRequest request, long? ownerId = null, CancellationToken ct = default);
}
