using LanguageLMS.Core.Modules.Quiz.Requests;
using LanguageLMS.Core.Modules.Quiz.Responses;

namespace LanguageLMS.Core.Modules.Quiz.Abstractions;

public interface IStudentQuizService
{
    Task<StartQuizResponse> StartAsync(long studentId, StartQuizRequest request, CancellationToken ct = default);

    Task<CompleteQuizResponse> CompleteAsync(long studentId, CompleteQuizRequest request, CancellationToken ct = default);

    /// <summary>
    /// Tələbənin tamamlanmış cəhdlərini qaytarır. <paramref name="from"/> (daxil) və
    /// <paramref name="to"/> (xaric) verilərsə tarix aralığı üzrə süzgəc tətbiq olunur.
    /// </summary>
    Task<IReadOnlyList<QuizResultSummary>> ListResultsAsync(
        long studentId, DateTimeOffset? from = null, DateTimeOffset? to = null, CancellationToken ct = default);

    Task<QuizResultDetail> GetResultAsync(long studentId, long attemptId, CancellationToken ct = default);
}
