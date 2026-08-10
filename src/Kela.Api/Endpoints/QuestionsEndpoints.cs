using Kela.Api.Contracts;
using Kela.Application.Features.Questions;
using Kela.Application.Features.Questions.Requests;
using Kela.Application.Features.Questions.Responses;
using Kela.Domain.Common;

namespace Kela.Api.Endpoints;

public static class QuestionsEndpoints
{
    public static IEndpointRouteBuilder MapQuestionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/questions")
            .RequireAuthorization(policy => policy.RequireRole(RoleNames.Teacher, RoleNames.Admin));

        group.MapGet("", async (int teacherId, IQuestionService questions, CancellationToken ct) =>
            ApiResponse<List<QuestionResponse>>.Success(
                await questions.GetByTeacherAsync(teacherId, ct)));

        group.MapGet("/{id:int}", async (int id, IQuestionService questions, CancellationToken ct) =>
        {
            var question = await questions.GetByIdAsync(id, ct);
            return question is null
                ? ApiResponse.NotFound("Soru bulunamadı.")
                : ApiResponse<QuestionResponse>.Success(question);
        });

        group.MapPost("", async (CreateQuestionRequest request, IQuestionService questions, CancellationToken ct) =>
        {
            var id = await questions.CreateAsync(request, ct);
            return ApiResponse<int>.Created($"/api/questions/{id}", id);
        });

        group.MapPut("/{id:int}", async (int id, UpdateQuestionRequest request, IQuestionService questions, CancellationToken ct) =>
        {
            await questions.UpdateAsync(id, request, ct);
            return ApiResponse.NoContent();
        });

        group.MapDelete("/{id:int}", async (int id, IQuestionService questions, CancellationToken ct) =>
        {
            await questions.DeleteAsync(id, ct);
            return ApiResponse.NoContent();
        });

        return app;
    }
}
