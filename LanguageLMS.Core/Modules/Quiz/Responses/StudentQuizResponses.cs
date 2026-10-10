namespace LanguageLMS.Core.Modules.Quiz.Responses;

public record QuizQuestionForStudent(
    long Id,
    string Title,
    string? VariantA,
    string? VariantB,
    string? VariantC,
    string? VariantD,
    string? VariantE);

public record StartQuizResponse(
    long AttemptId,
    long QuizId,
    string Title,
    int DurationMinutes,
    int TotalQuestions,
    IReadOnlyList<QuizQuestionForStudent> Questions);

public record CompleteQuizResponse(
    long AttemptId,
    int TotalQuestions,
    int CorrectCount,
    int ScorePercent,
    bool IsCompleted);

public record QuizResultItem(
    long QuestionId,
    string Title,
    string? StudentAnswer,
    string CorrectAnswer,
    bool IsRight);

public record QuizResultSummary(
    long AttemptId,
    long QuizId,
    string QuizTitle,
    int TotalQuestions,
    int CorrectCount,
    int ScorePercent,
    bool IsCompleted);

public record QuizResultDetail(
    long AttemptId,
    long QuizId,
    string QuizTitle,
    int TotalQuestions,
    int CorrectCount,
    int ScorePercent,
    bool IsCompleted,
    DateTimeOffset? StartDate,
    DateTimeOffset? EndDate,
    IReadOnlyList<QuizResultItem> Items);
