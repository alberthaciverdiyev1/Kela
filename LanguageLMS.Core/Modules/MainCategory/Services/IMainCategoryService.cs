using LanguageLMS.Core.Common.Responses;
using LanguageLMS.Core.Modules.MainCategory.Requests;
using LanguageLMS.Core.Modules.MainCategory.Responses;

namespace LanguageLMS.Core.Modules.MainCategory.Services;

public interface IMainCategoryService
{
    Task<PagedResponse<MainCategoryResponse>> ListAsync(MainCategoryListRequest request, CancellationToken ct = default);

    Task<MainCategoryResponse> GetAsync(long id, CancellationToken ct = default);

    Task<MainCategoryResponse> CreateAsync(MainCategoryUpsertRequest request, CancellationToken ct = default);

    Task<MainCategoryResponse> UpdateAsync(long id, MainCategoryUpsertRequest request, CancellationToken ct = default);

    Task DeleteAsync(long id, CancellationToken ct = default);
}
