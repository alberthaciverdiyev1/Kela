namespace LanguageLMS.App.Shared.Models;

public record Student(long Id, string FullName, string Email, string? Phone)
{
    public string FirstName => string.IsNullOrWhiteSpace(FullName) ? "şagird" : FullName.Trim().Split(' ')[0];
    public string Initial => FirstName.Length > 0 ? FirstName[..1].ToUpper() : "L";
}

public record AuthResult(string Token, Student User);

public record Group(long Id, string Name, string? Description, int StudentCount);

public record Subject(long Id, string Name);

public record Lesson(long Id, string Title, int SortOrder);

public record ContentItem(long Id, string Title, string? Description, string? Type, string? Url,
    string? PlaybackUrl, int? DurationSeconds)
{
    public string TypeLabel => Type switch
    {
        "video" => "Video",
        "file" => "Fayl",
        "voice" => "Səs yazısı",
        _ => "Material"
    };
}

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

public record ProgressContentRef(long ContentId, string Title);

public record ProgressItem(long LessonId, string LessonTitle, int TotalContents, int CompletedContents,
    int ProgressPercent, ProgressContentRef? NextContent);

public record ProgressOverview(string Type, List<ProgressItem> Items);

public record StatisticsOverview(int TotalLessons, int TotalContents, int CompletedContents, int CompletionPercent);

public record SubjectStat(long SubjectId, string SubjectName, int TotalContents, int CompletedContents, int CompletionPercent);

public record Statistics(StatisticsOverview Overview, List<SubjectStat> BySubject);
