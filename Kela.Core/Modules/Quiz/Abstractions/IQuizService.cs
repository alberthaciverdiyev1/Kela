using Kela.Core.Common.Bulk;
using Kela.Core.Common.Responses;
using Kela.Core.Common.Sorting;
using Kela.Core.Modules.Question.Responses;
using Kela.Core.Modules.Quiz.Requests;
using Kela.Core.Modules.Quiz.Responses;

namespace Kela.Core.Modules.Quiz.Abstractions;

public interface IQuizService
{
    Task<PagedResponse<QuizResponse>> ListAsync(QuizListRequest request, CancellationToken ct = default);

    Task<QuizResponse> GetAsync(long id, CancellationToken ct = default);

    Task<QuizResponse> CreateAsync(QuizUpsertRequest request, CancellationToken ct = default);

    Task<QuizResponse> UpdateAsync(long id, QuizUpsertRequest request, CancellationToken ct = default);

    Task DeleteAsync(long id, CancellationToken ct = default);

    Task<IReadOnlyList<QuestionResponse>> ListQuestionsAsync(long quizId, CancellationToken ct = default);

    Task<IReadOnlyList<QuestionResponse>> AssignQuestionsAsync(long quizId, AssignQuestionsRequest request, CancellationToken ct = default);

    Task RemoveQuestionAsync(long quizId, long questionId, CancellationToken ct = default);

    Task BulkAsync(BulkRequest request, CancellationToken ct = default);

    Task ReorderAsync(SortRequest request, CancellationToken ct = default);
}
