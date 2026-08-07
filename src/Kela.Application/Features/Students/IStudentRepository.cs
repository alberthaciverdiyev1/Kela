using Kela.Application.Pagination;
using Kela.Domain.Entities;

namespace Kela.Application.Features.Students;

public interface IStudentRepository
{
    Task<StudentProfile?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PaginatedResult<StudentProfile>> GetPageAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default);

    void Add(StudentProfile student);
    void Update(StudentProfile student);
    void Remove(StudentProfile student);
}
