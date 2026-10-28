namespace Edutor.App.Shared.Dtos.Homework;

public record HomeworkFileDto(long Id, string FileName, string Url, string? ContentType, long SizeBytes);

public record HomeworkQuizDto(
    long QuizId,
    string Title,
    int QuestionCount,
    int DurationMinutes,
    long? AttemptId,
    bool Completed,
    int CorrectCount,
    int ScorePercent);

public record StudentHomeworkSummaryDto(
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

public record StudentHomeworkDetailDto(
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
    IReadOnlyList<HomeworkFileDto> Files,
    IReadOnlyList<HomeworkQuizDto> Quizzes,
    bool HasQuizzes,
    bool AllQuizzesCompleted,
    bool CanSubmit);

public record HomeworkUploadFile(Stream Content, string FileName, string ContentType);
