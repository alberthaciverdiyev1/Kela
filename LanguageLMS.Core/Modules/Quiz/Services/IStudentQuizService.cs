using LanguageLMS.Core.Modules.Quiz.Requests;
using LanguageLMS.Core.Modules.Quiz.Responses;

namespace LanguageLMS.Core.Modules.Quiz.Services;

public interface IStudentQuizService
{
    Task<StartQuizResponse> StartAsync(long studentId, StartQuizRequest request, CancellationToken ct = default);

    Task<CompleteQuizResponse> CompleteAsync(long studentId, CompleteQuizRequest request, CancellationToken ct = default);

    Task<IReadOnlyList<QuizResultSummary>> ListResultsAsync(long studentId, CancellationToken ct = default);

    Task<QuizResultDetail> GetResultAsync(long studentId, long attemptId, CancellationToken ct = default);
}
