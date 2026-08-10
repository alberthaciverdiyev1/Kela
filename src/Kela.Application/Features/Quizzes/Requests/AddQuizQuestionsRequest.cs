namespace Kela.Application.Features.Quizzes.Requests;

public sealed record AddQuizQuestionsRequest(IReadOnlyList<int> QuestionIds);
