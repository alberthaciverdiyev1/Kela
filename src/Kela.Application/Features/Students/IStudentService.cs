using Kela.Application.Features.Students.Requests;
using Kela.Application.Features.Students.Responses;
using Kela.Application.Pagination;

namespace Kela.Application.Features.Students;

public interface IStudentService
{
    Task<PaginatedResult<StudentResponse>> GetPageAsync(
        int page, string? search, string? language, CancellationToken cancellationToken = default);

    Task<StudentResponse?> GetByIdAsync(int id, string? language, CancellationToken cancellationToken = default);

    Task<StudentCreatedResponse> CreateAsync(CreateStudentRequest request, CancellationToken cancellationToken = default);

    Task UpdateAsync(int id, UpdateStudentRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
