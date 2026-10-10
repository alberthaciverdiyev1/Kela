using LanguageLMS.Core.Common.Responses;
using LanguageLMS.Core.Modules.Profession.Requests;
using LanguageLMS.Core.Modules.Profession.Responses;

namespace LanguageLMS.Core.Modules.Profession.Services;

public interface IProfessionService
{
    Task<PagedResponse<ProfessionResponse>> ListAsync(ProfessionListRequest request, CancellationToken ct = default);

    Task<ProfessionResponse> GetAsync(long id, CancellationToken ct = default);

    Task<ProfessionResponse> CreateAsync(ProfessionUpsertRequest request, CancellationToken ct = default);

    Task<ProfessionResponse> UpdateAsync(long id, ProfessionUpsertRequest request, CancellationToken ct = default);

    Task DeleteAsync(long id, CancellationToken ct = default);
}
