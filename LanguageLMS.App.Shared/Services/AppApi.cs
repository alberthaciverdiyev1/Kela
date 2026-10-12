using LanguageLMS.App.Shared.Models;

namespace LanguageLMS.App.Shared.Services;

/// <summary>Student app-in bütün API çağırışları bir yerdə.</summary>
public sealed class AppApi(ApiClient api)
{
    // --- Auth ---
    public async Task<Student> LoginAsync(string email, string password)
    {
        var result = await api.PostAsync<AuthResult>("/api/auth/login", new { email, password });
        api.Token = result.Token;
        return result.User;
    }

    public async Task<Student> RegisterAsync(string fullName, string email, string? phone, string password)
    {
        var result = await api.PostAsync<AuthResult>("/api/auth/register",
            new { fullName, email, phone, password });
        api.Token = result.Token;
        return result.User;
    }

    public Task<Student> MeAsync() => api.GetAsync<Student>("/api/auth/me");
    public Task LogoutAsync() => api.PostAsync<object>("/api/auth/logout");

    // --- Catalog ---
    public Task<List<Group>> GroupsAsync() => api.GetAsync<List<Group>>("/api/student/groups");
    public Task<List<Subject>> SubjectsAsync(long groupId) => api.GetAsync<List<Subject>>($"/api/student/groups/{groupId}/subjects");
    public Task<List<Lesson>> LessonsAsync(long subjectId) => api.GetAsync<List<Lesson>>($"/api/student/subjects/{subjectId}/lessons");
    public Task<List<ContentItem>> ContentsAsync(long lessonId) => api.GetAsync<List<ContentItem>>($"/api/student/lessons/{lessonId}/contents");

    // --- Quizzes ---
    public Task<List<QuizItem>> QuizzesAsync() => api.GetAsync<List<QuizItem>>("/api/student/quizzes");
    public Task<List<QuizItem>> LessonQuizzesAsync(long lessonId) => api.GetAsync<List<QuizItem>>($"/api/student/lessons/{lessonId}/quizzes");
    public Task<StartQuizResult> StartQuizAsync(long quizId) => api.PostAsync<StartQuizResult>("/api/student-quiz/start", new { quizId });
    public Task<CompleteQuizResult> CompleteQuizAsync(long attemptId, IEnumerable<(long QuestionId, string? Answer)> answers)
        => api.PostAsync<CompleteQuizResult>("/api/student-quiz/complete", new
        {
            attemptId,
            answers = answers.Select(a => new { questionId = a.QuestionId, answer = a.Answer })
        });
    public Task<List<QuizResultSummary>> ResultsAsync() => api.GetAsync<List<QuizResultSummary>>("/api/student-quiz/results");

    // --- Progress ---
    public Task<ProgressOverview> ProgressAsync() => api.GetAsync<ProgressOverview>("/api/progress");
    public Task<Statistics> StatisticsAsync() => api.GetAsync<Statistics>("/api/statistics");
    public Task MarkCompletedAsync(long contentId) => api.PostAsync<object>("/api/progress", new { contentId });

    public Task<List<BannerResponse>> BannersAsync() => api.GetAsync<List<BannerResponse>>("/api/banner");

    public Task<List<GroupResponse>> StudentGroupsAsync() => api.GetAsync<List<GroupResponse>>("/api/student/groups");
}
