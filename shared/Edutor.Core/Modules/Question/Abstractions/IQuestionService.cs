using Edutor.Core.Common.Bulk;
using Edutor.Core.Common.Responses;
using Edutor.Core.Modules.Question.Requests;
using Edutor.Core.Modules.Question.Responses;

namespace Edutor.Core.Modules.Question.Abstractions;

public interface IQuestionService
{
    Task<PagedResponse<QuestionResponse>> ListAsync(QuestionListRequest request, CancellationToken ct = default);

    /// <summary><paramref name="ownerId"/> verilərsə yalnız öz/admin sualına giriş icazəsi verilir.</summary>
    Task<QuestionResponse> GetAsync(long id, long? ownerId = null, CancellationToken ct = default);

    Task<QuestionResponse> CreateAsync(QuestionUpsertRequest request, CancellationToken ct = default);

    Task<QuestionResponse> UpdateAsync(long id, QuestionUpsertRequest request, long? ownerId = null, CancellationToken ct = default);

    Task DeleteAsync(long id, long? ownerId = null, CancellationToken ct = default);

    Task BulkAsync(BulkRequest request, long? ownerId = null, CancellationToken ct = default);
}
