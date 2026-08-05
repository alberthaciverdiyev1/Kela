using Kela.Application.Repositories;
using Kela.Domain.Grades;
using Kela.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kela.Infrastructure.Repositories;

internal sealed class GradeRepository(KelaDbContext context) : IGradeRepository
{
    private IQueryable<Grade> Detailed => context.Grades
        .Include(g => g.Teacher!)
        .ThenInclude(t => t.User)
        .Include(g => g.Students);

    public async Task<Grade?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await Detailed.FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

    public async Task<List<Grade>> GetAllAsync(CancellationToken cancellationToken = default)
        => await Detailed.AsNoTracking().ToListAsync(cancellationToken);

    public Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default)
        => context.Grades.AnyAsync(g => g.Name == name, cancellationToken);

    public void Add(Grade grade) => context.Grades.Add(grade);

    public void Update(Grade grade) => context.Grades.Update(grade);

    public void Remove(Grade grade) => context.Grades.Remove(grade);
}
