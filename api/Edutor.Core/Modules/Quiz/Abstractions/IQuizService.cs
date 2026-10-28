using Edutor.Core.Common.Bulk;
using Edutor.Core.Common.Responses;
using Edutor.Core.Common.Sorting;
using Edutor.Core.Modules.Question.Responses;
using Edutor.Core.Modules.Quiz.Requests;
using Edutor.Core.Modules.Quiz.Responses;

namespace Edutor.Core.Modules.Quiz.Abstractions;

public interface IQuizService
{
    Task<PagedResponse<QuizResponse>> ListAsync(QuizListRequest request, CancellationToken ct = default);

    /// <summary><paramref name="ownerId"/> verilərsə yalnız öz/admin imtahanına giriş icazəsi verilir.</summary>
    Task<QuizResponse> GetAsync(long id, long? ownerId = null, CancellationToken ct = default);

    Task<QuizResponse> CreateAsync(QuizUpsertRequest request, CancellationToken ct = default);

    Task<QuizResponse> UpdateAsync(long id, QuizUpsertRequest request, long? ownerId = null, CancellationToken ct = default);

    Task DeleteAsync(long id, long? ownerId = null, CancellationToken ct = default);

    Task<IReadOnlyList<QuestionResponse>> ListQuestionsAsync(long quizId, long? ownerId = null, CancellationToken ct = default);

    Task<IReadOnlyList<QuestionResponse>> AssignQuestionsAsync(long quizId, AssignQuestionsRequest request, long? ownerId = null, CancellationToken ct = default);

    Task RemoveQuestionAsync(long quizId, long questionId, long? ownerId = null, CancellationToken ct = default);

    Task<IReadOnlyList<QuizGroupResponse>> ListGroupsAsync(long quizId, long? ownerId = null, CancellationToken ct = default);

    /// <summary>İmtahana bağlana bilən qruplar — imtahanın fənni qrupda varsa.</summary>
    Task<IReadOnlyList<QuizGroupResponse>> ListGroupOptionsAsync(long quizId, long? ownerId = null, CancellationToken ct = default);

    Task<IReadOnlyList<QuizGroupResponse>> AssignGroupsAsync(long quizId, AssignGroupsRequest request, long? ownerId = null, CancellationToken ct = default);

    Task RemoveGroupAsync(long quizId, long groupId, long? ownerId = null, CancellationToken ct = default);

    Task SetActiveAsync(long id, bool isActive, long? ownerId = null, CancellationToken ct = default);

    Task BulkAsync(BulkRequest request, long? ownerId = null, CancellationToken ct = default);

    Task ReorderAsync(SortRequest request, long? ownerId = null, CancellationToken ct = default);
}
