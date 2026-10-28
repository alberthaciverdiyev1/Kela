using Edutor.Core.Common.Bulk;
using Edutor.Core.Common.Responses;
using Edutor.Core.Modules.Subject.Requests;
using Edutor.Core.Modules.Subject.Responses;

namespace Edutor.Core.Modules.Subject.Abstractions;

public interface ISubjectService
{
    Task<PagedResponse<SubjectResponse>> ListAsync(SubjectListRequest request, CancellationToken ct = default);

    /// <summary><paramref name="ownerId"/> verilərsə yalnız öz/admin qeydinə giriş icazəsi verilir.</summary>
    Task<SubjectResponse> GetAsync(long id, long? ownerId = null, CancellationToken ct = default);

    Task<SubjectResponse> CreateAsync(SubjectUpsertRequest request, CancellationToken ct = default);

    Task<SubjectResponse> UpdateAsync(long id, SubjectUpsertRequest request, long? ownerId = null, CancellationToken ct = default);

    Task DeleteAsync(long id, long? ownerId = null, CancellationToken ct = default);

    Task BulkAsync(BulkRequest request, long? ownerId = null, CancellationToken ct = default);
}
