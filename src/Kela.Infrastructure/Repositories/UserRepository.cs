using Kela.Application.Features.Users;
using Kela.Application.Pagination;
using Kela.Domain.Entities;
using Kela.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kela.Infrastructure.Repositories;

internal sealed class UserRepository(KelaDbContext context) : IUserRepository
{
    private IQueryable<User> Profiles => context.Users
        .Include(u => u.Teacher)
        .Include(u => u.Student)
        .Include(u => u.Parent);

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await Profiles.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<PaginatedResult<User>> GetPageAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = Profiles.AsNoTracking();
        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(u => u.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<User>(items, page, pageSize, total);
    }
}
