namespace Kela.Application.Features.Questions.Requests;

public sealed record UpdateQuestionRequest(
    string Text,
    string OptionA,
    string OptionB,
    string OptionC,
    string? OptionD,
    string? OptionE,
    int CorrectOption);
