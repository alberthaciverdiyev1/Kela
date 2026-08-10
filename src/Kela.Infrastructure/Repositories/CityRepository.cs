using Kela.Application.Features.Cities;
using Kela.Application.Pagination;
using Kela.Domain.Entities;
using Kela.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kela.Infrastructure.Repositories;

internal sealed class CityRepository(KelaDbContext context) : ICityRepository
{
    public async Task<City?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await context.Cities.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<PaginatedResult<City>> GetPageAsync(
        int page, CancellationToken cancellationToken = default)
    {
        var query = context.Cities.AsNoTracking();
        var total = await query.CountAsync(cancellationToken);
        var pageSize = PaginationDefaults.PageSize;

        var items = await query
            .OrderBy(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<City>(items, page, pageSize, total);
    }

    public void Add(City city) => context.Cities.Add(city);

    public void Update(City city) => context.Cities.Update(city);

    public void Remove(City city) => context.Cities.Remove(city);
}
