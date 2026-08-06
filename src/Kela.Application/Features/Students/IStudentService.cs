using Kela.Application.Features.Students.Requests;
using Kela.Application.Features.Students.Responses;
using Kela.Application.Pagination;

namespace Kela.Application.Features.Students;

public interface IStudentService
{
    Task<PaginatedResult<StudentResponse>> GetPageAsync(
        int page, int pageSize, string? language, CancellationToken cancellationToken = default);

    Task<StudentResponse?> GetByIdAsync(int id, string? language, CancellationToken cancellationToken = default);

    /// <summary>Öğrenci oluşturur: Student rolünde User + bağlı StudentProfile.</summary>
    Task<int> CreateAsync(CreateStudentRequest request, CancellationToken cancellationToken = default);

    Task UpdateAsync(int id, UpdateStudentRequest request, CancellationToken cancellationToken = default);

    /// <summary>Soft delete — User'ı Inactive yapar, profil mantıken silinir.</summary>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
