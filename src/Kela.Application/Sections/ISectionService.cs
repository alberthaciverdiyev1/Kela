using Kela.Application.Pagination;
using Kela.Application.Sections.Dtos;

namespace Kela.Application.Sections;

public interface ISectionService
{
    Task<PaginatedResult<SectionDto>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<SectionDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(string name, int level, int? teacherId, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, string name, int level, int? teacherId, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
