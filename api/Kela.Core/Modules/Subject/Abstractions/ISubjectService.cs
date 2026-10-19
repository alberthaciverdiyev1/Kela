using Kela.Core.Common.Bulk;
using Kela.Core.Common.Responses;
using Kela.Core.Modules.Subject.Requests;
using Kela.Core.Modules.Subject.Responses;

namespace Kela.Core.Modules.Subject.Abstractions;

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
