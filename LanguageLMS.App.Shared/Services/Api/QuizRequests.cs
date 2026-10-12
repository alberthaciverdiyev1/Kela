using LanguageLMS.App.Shared.Models;

namespace LanguageLMS.App.Shared.Services.Api;

public sealed partial class AppApi
{
    // --- Quizzes ---
    public Task<List<QuizItem>> QuizzesAsync() => _api.GetAsync<List<QuizItem>>("/api/student/quizzes");
    public Task<List<QuizItem>> LessonQuizzesAsync(long lessonId) => _api.GetAsync<List<QuizItem>>($"/api/student/lessons/{lessonId}/quizzes");

    public Task<StartQuizResult> StartQuizAsync(long quizId)
        => _api.PostAsync<StartQuizResult>("/api/student-quiz/start", new { quizId });

    public Task<CompleteQuizResult> CompleteQuizAsync(long attemptId, IEnumerable<(long QuestionId, string? Answer)> answers)
        => _api.PostAsync<CompleteQuizResult>("/api/student-quiz/complete", new
        {
            attemptId,
            answers = answers.Select(a => new { questionId = a.QuestionId, answer = a.Answer })
        });

    public Task<List<QuizResultSummary>> ResultsAsync() => _api.GetAsync<List<QuizResultSummary>>("/api/student-quiz/results");
    public Task<QuizResultDetail> ResultDetailAsync(long attemptId) => _api.GetAsync<QuizResultDetail>($"/api/student-quiz/results/{attemptId}");
}
