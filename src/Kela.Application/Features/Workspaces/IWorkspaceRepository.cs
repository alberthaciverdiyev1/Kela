using Kela.Application.Pagination;
using Kela.Domain.Entities;

namespace Kela.Application.Features.Workspaces;

public interface IWorkspaceRepository
{
    Task<Workspace?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PaginatedResult<Workspace>> GetPageAsync(int teacherId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(int teacherId, string name, CancellationToken cancellationToken = default);

    void Add(Workspace workspace);
    void Update(Workspace workspace);
    void Remove(Workspace workspace);
}
