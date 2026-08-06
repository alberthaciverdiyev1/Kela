using Kela.Application.Pagination;
using Kela.Domain.Entities;

namespace Kela.Application.Features.Sections;

public interface ISectionRepository
{
    Task<Section?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PaginatedResult<Section>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default);

    void Add(Section section);
    void Update(Section section);
    void Remove(Section section);
}
