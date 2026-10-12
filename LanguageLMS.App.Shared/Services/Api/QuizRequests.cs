using LanguageLMS.App.Shared.Dtos;
using LanguageLMS.App.Shared.Dtos.Auth;
using LanguageLMS.App.Shared.Dtos.Progress;
using LanguageLMS.App.Shared.Dtos.Quiz;

namespace LanguageLMS.App.Shared.Services.Api;

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

    public Task<List<QuizResultSummaryDto>> ResultsAsync() => _api.GetAsync<List<QuizResultSummaryDto>>("/api/student-quiz/results");
    public Task<QuizResultDetailDto> ResultDetailAsync(long attemptId) => _api.GetAsync<QuizResultDetailDto>($"/api/student-quiz/results/{attemptId}");
}
