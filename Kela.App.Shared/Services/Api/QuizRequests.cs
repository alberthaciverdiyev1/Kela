using Kela.App.Shared.Dtos;
using Kela.App.Shared.Dtos.Auth;
using Kela.App.Shared.Dtos.Progress;
using Kela.App.Shared.Dtos.Quiz;

namespace Kela.App.Shared.Services.Api;

public sealed partial class AppApi
{
    // --- Quizzes ---
    public Task<List<QuizDto>> QuizzesAsync() => _api.GetAsync<List<QuizDto>>("/api/student/quizzes");
    public Task<List<QuizDto>> LessonQuizzesAsync(long lessonId) => _api.GetAsync<List<QuizDto>>($"/api/student/lessons/{lessonId}/quizzes");

    public Task<StartQuizResultDto> StartQuizAsync(long quizId)
        => _api.PostAsync<StartQuizResultDto>("/api/student-quiz/start", new { quizId });

    public Task<CompleteQuizResultDto> CompleteQuizAsync(long attemptId, IEnumerable<(long QuestionId, string? Answer)> answers)
        => _api.PostAsync<CompleteQuizResultDto>("/api/student-quiz/complete", new
        {
            attemptId,
            answers = answers.Select(a => new { questionId = a.QuestionId, answer = a.Answer })
        });

    public Task<List<QuizResultSummaryDto>> ResultsAsync(DateTimeOffset? from = null, DateTimeOffset? to = null)
    {
        var query = new List<string>(2);
        if (from is not null) query.Add($"from={Uri.EscapeDataString(from.Value.ToString("o"))}");
        if (to is not null) query.Add($"to={Uri.EscapeDataString(to.Value.ToString("o"))}");
        var qs = query.Count > 0 ? "?" + string.Join("&", query) : string.Empty;
        return _api.GetAsync<List<QuizResultSummaryDto>>($"/api/student-quiz/results{qs}");
    }
    public Task<QuizResultDetailDto> ResultDetailAsync(long attemptId) => _api.GetAsync<QuizResultDetailDto>($"/api/student-quiz/results/{attemptId}");
}
