namespace LanguageLMS.Core.Modules.Question.Responses;

public record QuestionResponse(
    long Id,
    long SubjectId,
    string Title,
    string? VariantA,
    string? VariantB,
    string? VariantC,
    string? VariantD,
    string? VariantE,
    string CorrectAnswer,
    string VariantType);
