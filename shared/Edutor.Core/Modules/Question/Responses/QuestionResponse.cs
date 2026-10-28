namespace Edutor.Core.Modules.Question.Responses;

public record QuestionResponse(
    long Id,
    long SubjectId,
    string? Title,
    string TitleType,
    string? TitleUrl,
    string? VariantA,
    string? VariantAUrl,
    string? VariantB,
    string? VariantBUrl,
    string? VariantC,
    string? VariantCUrl,
    string? VariantD,
    string? VariantDUrl,
    string? VariantE,
    string? VariantEUrl,
    string CorrectAnswer,
    string VariantType);
