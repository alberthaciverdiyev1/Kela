using Kela.Application.Pagination;
using Kela.Application.Repositories;
using Kela.Domain.Sections;
using Kela.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kela.Infrastructure.Repositories;

internal sealed class SectionRepository(KelaDbContext context) : ISectionRepository
{
    private IQueryable<Section> Detailed => context.Sections
        .Include(g => g.Teacher!)
        .ThenInclude(t => t.User)
        .Include(g => g.Students);

    public async Task<Section?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await Detailed.FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

    public async Task<PaginatedResult<Section>> GetPageAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = Detailed.AsNoTracking();
        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(g => g.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<Section>(items, page, pageSize, total);
    }

    public Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default)
        => context.Sections.AnyAsync(g => g.Name == name, cancellationToken);

    public void Add(Section section) => context.Sections.Add(section);

    public void Update(Section section) => context.Sections.Update(section);

    public void Remove(Section section) => context.Sections.Remove(section);
}
