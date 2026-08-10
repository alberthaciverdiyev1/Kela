using Kela.Api.Contracts;
using Kela.Application.Features.Quizzes;
using Kela.Application.Features.Quizzes.Requests;
using Kela.Application.Features.Quizzes.Responses;
using Kela.Domain.Common;

namespace Kela.Api.Endpoints;

public static class QuizzesEndpoints
{
    public static IEndpointRouteBuilder MapQuizzesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/quizzes")
            .RequireAuthorization(policy => policy.RequireRole(RoleNames.Teacher, RoleNames.Admin));

        group.MapGet("", async (int teacherId, IQuizService quizzes, CancellationToken ct) =>
            ApiResponse<List<QuizResponse>>.Success(
                await quizzes.GetByTeacherAsync(teacherId, ct)));

        group.MapGet("/{contentId:int}", async (int contentId, IQuizService quizzes, CancellationToken ct) =>
        {
            var quiz = await quizzes.GetByContentIdAsync(contentId, ct);
            return quiz is null
                ? ApiResponse.NotFound("Quiz bulunamadı.")
                : ApiResponse<QuizResponse>.Success(quiz);
        });

        group.MapPost("/{contentId:int}/questions", async (int contentId, AddQuizQuestionsRequest request, IQuizService quizzes, CancellationToken ct) =>
        {
            await quizzes.AddQuestionsAsync(contentId, request, ct);
            return ApiResponse.NoContent();
        });

        group.MapDelete("/{contentId:int}/questions/{questionId:int}", async (int contentId, int questionId, IQuizService quizzes, CancellationToken ct) =>
        {
            await quizzes.RemoveQuestionAsync(contentId, questionId, ct);
            return ApiResponse.NoContent();
        });

        return app;
    }
}
