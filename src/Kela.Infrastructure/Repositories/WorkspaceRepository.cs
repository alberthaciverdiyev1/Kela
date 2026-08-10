using Kela.Application.Features.Workspaces;
using Kela.Application.Pagination;
using Kela.Domain.Entities;
using Kela.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kela.Infrastructure.Repositories;

internal sealed class WorkspaceRepository(KelaDbContext context) : IWorkspaceRepository
{
    private IQueryable<Workspace> Detailed => context.Workspaces
        .Include(w => w.Teacher)
        .Include(w => w.Students);

    public async Task<Workspace?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await Detailed.FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

    public async Task<PaginatedResult<Workspace>> GetPageAsync(
        int teacherId, int page, string? search, CancellationToken cancellationToken = default)
    {
        var query = Detailed.AsNoTracking().Where(w => w.TeacherId == teacherId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(w => w.Name.ToLower().Contains(s.ToLower()));
        }

        var total = await query.CountAsync(cancellationToken);
        var pageSize = PaginationDefaults.PageSize;

        var items = await query
            .OrderBy(w => w.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<Workspace>(items, page, pageSize, total);
    }

    public Task<bool> NameExistsAsync(int teacherId, string name, CancellationToken cancellationToken = default)
        => context.Workspaces.AnyAsync(w => w.TeacherId == teacherId && w.Name == name, cancellationToken);

    public void Add(Workspace workspace) => context.Workspaces.Add(workspace);

    public void Update(Workspace workspace) => context.Workspaces.Update(workspace);

    public void Remove(Workspace workspace) => context.Workspaces.Remove(workspace);
}
