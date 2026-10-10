using LanguageLMS.Core.Common.Responses;
using LanguageLMS.Core.Modules.Question.Responses;
using LanguageLMS.Core.Modules.Quiz.Requests;
using LanguageLMS.Core.Modules.Quiz.Responses;

namespace LanguageLMS.Core.Modules.Quiz.Abstractions;

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
}
