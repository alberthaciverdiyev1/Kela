namespace Kela.Web.Models.Quizzes;

public sealed record QuizEditorViewModel(
    int ContentId,
    string Title,
    string? Description,
    bool IsPublished);
