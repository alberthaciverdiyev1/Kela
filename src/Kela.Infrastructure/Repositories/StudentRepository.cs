using Kela.Application.Features.Students;
using Kela.Application.Pagination;
using Kela.Domain.Entities;
using Kela.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kela.Infrastructure.Repositories;

internal sealed class StudentRepository(KelaDbContext context) : IStudentRepository
{
    private IQueryable<StudentProfile> Detailed => context.StudentProfiles
        .Include(p => p.User)
        .Include(p => p.City);

    public async Task<StudentProfile?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await Detailed.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<PaginatedResult<StudentProfile>> GetPageAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = Detailed.AsNoTracking();
        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<StudentProfile>(items, page, pageSize, total);
    }

    public void Add(StudentProfile student) => context.StudentProfiles.Add(student);

    public void Update(StudentProfile student) => context.StudentProfiles.Update(student);

    public void Remove(StudentProfile student) => context.StudentProfiles.Remove(student);
}
