using Edutor.App.Shared.Dtos;
using Edutor.App.Shared.Dtos.Auth;
using Edutor.App.Shared.Dtos.Progress;
using Edutor.App.Shared.Dtos.Quiz;

namespace Edutor.App.Shared.Services.Api;

public sealed partial class AppApi
{
    // --- Quizzes ---
    public Task<List<QuizDto>> QuizzesAsync(long? subjectId = null, long? lessonId = null, bool general = false)
    {
        var query = new List<string>(3);
        if (subjectId is > 0) query.Add($"subject_id={subjectId}");
        if (lessonId is > 0) query.Add($"lesson_id={lessonId}");
        if (general) query.Add("general=true");
        var qs = query.Count > 0 ? "?" + string.Join("&", query) : string.Empty;
        return _api.GetAsync<List<QuizDto>>($"/api/student/quizzes{qs}");
    }

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
