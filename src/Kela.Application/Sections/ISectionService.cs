using Kela.Application.Pagination;
using Kela.Application.Sections.Requests;
using Kela.Application.Sections.Responses;

namespace Kela.Application.Sections;

public interface ISectionService
{
    Task<PaginatedResult<SectionResponse>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<SectionResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(CreateSectionRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, UpdateSectionRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
