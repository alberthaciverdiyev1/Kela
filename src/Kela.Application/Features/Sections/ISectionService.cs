using Kela.Application.Pagination;
using Kela.Application.Features.Sections.Requests;
using Kela.Application.Features.Sections.Responses;

namespace Kela.Application.Features.Sections;

public interface ISectionService
{
    Task<PaginatedResult<SectionResponse>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<SectionResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(CreateSectionRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, UpdateSectionRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
