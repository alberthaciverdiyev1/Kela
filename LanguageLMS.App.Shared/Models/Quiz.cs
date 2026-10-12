namespace LanguageLMS.App.Shared.Models;

public record QuizItem(long Id, string Title, int DurationMinutes, int QuestionCount, string Type);

public record QuizQuestion(long Id, string Title, string? VariantA, string? VariantB, string? VariantC,
    string? VariantD, string? VariantE)
{
    public IReadOnlyList<(string Key, string Text)> Options
    {
        get
        {
            var list = new List<(string, string)>();
            if (!string.IsNullOrWhiteSpace(VariantA)) list.Add(("A", VariantA!));
            if (!string.IsNullOrWhiteSpace(VariantB)) list.Add(("B", VariantB!));
            if (!string.IsNullOrWhiteSpace(VariantC)) list.Add(("C", VariantC!));
            if (!string.IsNullOrWhiteSpace(VariantD)) list.Add(("D", VariantD!));
            if (!string.IsNullOrWhiteSpace(VariantE)) list.Add(("E", VariantE!));
            return list;
        }
    }
}

public record StartQuizResult(long AttemptId, string Title, int TotalQuestions, List<QuizQuestion> Questions);

public record CompleteQuizResult(long AttemptId, int TotalQuestions, int CorrectCount, int ScorePercent);

public record QuizResultSummary(long AttemptId, string QuizTitle, int CorrectCount, int TotalQuestions, int ScorePercent);

public record QuizResultItem(long QuestionId, string Title, string? StudentAnswer, string CorrectAnswer, bool IsRight);

public record QuizResultDetail(long AttemptId, long QuizId, string QuizTitle, int TotalQuestions, int CorrectCount,
    int ScorePercent, bool IsCompleted, List<QuizResultItem> Items);
