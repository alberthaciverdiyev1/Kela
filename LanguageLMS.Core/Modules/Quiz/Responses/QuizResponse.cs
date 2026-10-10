namespace LanguageLMS.Core.Modules.Quiz.Responses;

public record QuizResponse(
    long Id,
    long LessonId,
    string Title,
    int DurationMinutes,
    string Type,
    bool IsActive,
    bool IsRepeat,
    int SortOrder,
    int QuestionCount);
