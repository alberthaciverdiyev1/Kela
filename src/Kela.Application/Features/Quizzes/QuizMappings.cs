using Kela.Application.Features.Questions;
using Kela.Application.Features.Quizzes.Responses;
using Kela.Domain.Entities;

namespace Kela.Application.Features.Quizzes;

public static class QuizMappings
{
    public static QuizResponse ToResponse(this Quiz quiz) => new(
        quiz.ContentId,
        quiz.TeacherId,
        quiz.Title,
        quiz.Description,
        quiz.IsPublished,
        quiz.Questions
            .Where(q => q.Question is not null)
            .OrderBy(q => q.Position)
            .Select(q => new QuizQuestionResponse(q.Position, q.Question!.ToResponse()))
            .ToList());
}
