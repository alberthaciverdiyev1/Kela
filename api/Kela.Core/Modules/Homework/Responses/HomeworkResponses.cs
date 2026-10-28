namespace Kela.Core.Modules.Homework.Responses;

public record HomeworkGroupInfoResponse(long Id, string Name, DateTimeOffset DueDate, decimal MaxScore);

public record HomeworkQuizInfoResponse(long Id, string Title, int QuestionCount, int DurationMinutes);

public record HomeworkResponse(
    long Id,
    string Title,
    string? Description,
    bool IsPublished,
    DateTimeOffset? PublishedAt,
    bool IsActive,
    long? UserId,
    IReadOnlyList<HomeworkGroupInfoResponse> Groups,
    IReadOnlyList<HomeworkQuizInfoResponse> Quizzes,
    int AssignedCount,
    int SubmittedCount,
    int GradedCount,
    int ReturnedCount);

public record HomeworkFileResponse(long Id, string FileName, string Url, string? ContentType, long SizeBytes);

public record HomeworkQuizResultResponse(
    long QuizId,
    string Title,
    int QuestionCount,
    int DurationMinutes,
    long? AttemptId,
    bool Completed,
    int CorrectCount,
    int ScorePercent);

public record HomeworkAssignmentResponse(
    long StudentId,
    string FullName,
    string Email,
    string Status,
    string? Text,
    string? Link,
    DateTimeOffset? SubmittedAt,
    bool IsLate,
    int AttemptCount,
    decimal? Score,
    string? Feedback,
    DateTimeOffset? GradedAt,
    DateTimeOffset? ReturnedAt,
    IReadOnlyList<HomeworkFileResponse> Files,
    IReadOnlyList<HomeworkQuizResultResponse> Quizzes);

public record StudentHomeworkSummaryResponse(
    long Id,
    string Title,
    DateTimeOffset DueDate,
    decimal MaxScore,
    string Status,
    bool IsLate,
    decimal? Score,
    DateTimeOffset? SubmittedAt,
    int QuizCount,
    int QuizzesCompleted);

public record StudentHomeworkDetailResponse(
    long Id,
    string Title,
    string? Description,
    DateTimeOffset DueDate,
    decimal MaxScore,
    string Status,
    bool IsLate,
    int AttemptCount,
    string? Text,
    string? Link,
    DateTimeOffset? SubmittedAt,
    decimal? Score,
    string? Feedback,
    DateTimeOffset? ReturnedAt,
    IReadOnlyList<HomeworkFileResponse> Files,
    IReadOnlyList<HomeworkQuizResultResponse> Quizzes,
    bool HasQuizzes,
    bool AllQuizzesCompleted,
    bool CanSubmit);
