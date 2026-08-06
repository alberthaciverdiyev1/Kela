using Kela.Api.Contracts;
using Kela.Application.Features.Students;
using Kela.Application.Features.Students.Requests;
using Kela.Application.Features.Students.Responses;
using Kela.Application.Pagination;
using Kela.Domain.Common;

namespace Kela.Api.Endpoints;

/// <summary>
/// Öğrenci yönetimi — Teacher (kendi panelinden) ve Admin (ayrı yönetim panelinden).
///   GET    /api/students?page=1&pageSize=10&lang=tr → sayfalı liste (şehir adı yerelleştirilmiş)
///   GET    /api/students/1?lang=tr                   → detay
///   POST   /api/students                             → User(Student) + StudentProfile oluşturur
///   PUT    /api/students/1                           → ad/soyad/şehir/doğum günceller
///   DELETE /api/students/1                           → soft delete (User Inactive)
/// </summary>
public static class StudentsEndpoints
{
    public static IEndpointRouteBuilder MapStudentsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/students")
            .RequireAuthorization(policy => policy.RequireRole(RoleNames.Teacher, RoleNames.Admin));

        group.MapGet("", async (int page, int pageSize, string? lang, IStudentService students, CancellationToken ct) =>
            ApiResponse<PaginatedResult<StudentResponse>>.Success(
                await students.GetPageAsync(page, pageSize, lang, ct)));

        group.MapGet("/{id:int}", async (int id, string? lang, IStudentService students, CancellationToken ct) =>
        {
            var student = await students.GetByIdAsync(id, lang, ct);
            return student is null
                ? ApiResponse.NotFound("Öğrenci bulunamadı.")
                : ApiResponse<StudentResponse>.Success(student);
        });

        group.MapPost("", async (CreateStudentRequest request, IStudentService students, CancellationToken ct) =>
        {
            var id = await students.CreateAsync(request, ct);
            return ApiResponse<object>.Created($"/api/students/{id}", new { id });
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
