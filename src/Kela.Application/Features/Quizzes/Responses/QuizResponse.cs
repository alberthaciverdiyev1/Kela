using Kela.Application.Features.Questions.Responses;

namespace Kela.Application.Features.Quizzes.Responses;

public sealed record QuizQuestionResponse(int Position, QuestionResponse Question);

public sealed record QuizResponse(
    int ContentId,
    int TeacherId,
    string Title,
    string? Description,
    bool IsPublished,
    IReadOnlyList<QuizQuestionResponse> Questions);
