using Kela.Core.Common.Bulk;
using Kela.Core.Common.Responses;
using Kela.Core.Modules.Subject.Requests;
using Kela.Core.Modules.Subject.Responses;

namespace Kela.Core.Modules.Subject.Abstractions;

public interface ISubjectService
{
    Task<PagedResponse<SubjectResponse>> ListAsync(SubjectListRequest request, CancellationToken ct = default);

    Task<SubjectResponse> GetAsync(long id, CancellationToken ct = default);

    Task<SubjectResponse> CreateAsync(SubjectUpsertRequest request, CancellationToken ct = default);

    Task<SubjectResponse> UpdateAsync(long id, SubjectUpsertRequest request, CancellationToken ct = default);

    Task DeleteAsync(long id, CancellationToken ct = default);

    Task BulkAsync(BulkRequest request, CancellationToken ct = default);
}
