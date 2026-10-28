namespace Edutor.Core.Modules.Quiz.Responses;

public record StudentQuizStat(
    long StudentId,
    string FullName,
    string Email,
    int QuizCount,
    int AttemptedQuizCount,
    int NotAttemptedQuizCount,
    int TotalQuestions,
    int CompletedQuestions,
    int Correct,
    int Wrong,
    double ScorePercent);

public record GroupQuizStatResponse(
    long GroupId,
    int QuizCount,
    int TotalQuestions,
    IReadOnlyList<StudentQuizStat> Students);
