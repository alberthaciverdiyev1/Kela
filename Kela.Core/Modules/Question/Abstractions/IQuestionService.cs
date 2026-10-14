using Kela.Core.Common.Bulk;
using Kela.Core.Common.Responses;
using Kela.Core.Modules.Question.Requests;
using Kela.Core.Modules.Question.Responses;

namespace Kela.Core.Modules.Question.Abstractions;

public interface IQuestionService
{
    Task<PagedResponse<QuestionResponse>> ListAsync(QuestionListRequest request, CancellationToken ct = default);

    Task<QuestionResponse> GetAsync(long id, CancellationToken ct = default);

    Task<QuestionResponse> CreateAsync(QuestionUpsertRequest request, CancellationToken ct = default);

    Task<QuestionResponse> UpdateAsync(long id, QuestionUpsertRequest request, CancellationToken ct = default);

    Task DeleteAsync(long id, CancellationToken ct = default);

    Task BulkAsync(BulkRequest request, CancellationToken ct = default);
}
