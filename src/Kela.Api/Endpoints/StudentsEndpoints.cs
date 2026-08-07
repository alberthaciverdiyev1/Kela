using Kela.Api.Contracts;
using Kela.Application.Features.Students;
using Kela.Application.Features.Students.Requests;
using Kela.Application.Features.Students.Responses;
using Kela.Application.Pagination;
using Kela.Domain.Common;

namespace Kela.Api.Endpoints;

public static class StudentsEndpoints
{
    public static IEndpointRouteBuilder MapStudentsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/students")
            .RequireAuthorization(policy => policy.RequireRole(RoleNames.Teacher, RoleNames.Admin));

        group.MapGet("", async (int page, int pageSize, string? search, string? lang, IStudentService students, CancellationToken ct) =>
            ApiResponse<PaginatedResult<StudentResponse>>.Success(
                await students.GetPageAsync(page, pageSize, search, lang, ct)));

        group.MapGet("/{id:int}", async (int id, string? lang, IStudentService students, CancellationToken ct) =>
        {
            var student = await students.GetByIdAsync(id, lang, ct);
            return student is null
                ? ApiResponse.NotFound("Öğrenci bulunamadı.")
                : ApiResponse<StudentResponse>.Success(student);
        });

        group.MapPost("", async (CreateStudentRequest request, IStudentService students, CancellationToken ct) =>
        {
            // Yanıt, sistemin ürettiği mail + şifreyi düz metin döndürür —
            // öğretmen öğrenciye iletmek için.
            var created = await students.CreateAsync(request, ct);
            return ApiResponse<StudentCreatedResponse>.Created($"/api/students/{created.Id}", created);
        });

        group.MapPut("/{id:int}", async (int id, UpdateStudentRequest request, IStudentService students, CancellationToken ct) =>
        {
            await students.UpdateAsync(id, request, ct);
            return ApiResponse.NoContent();
        });

        group.MapDelete("/{id:int}", async (int id, IStudentService students, CancellationToken ct) =>
        {
            await students.DeleteAsync(id, ct);
            return ApiResponse.NoContent();
        });

        return app;
    }
}
