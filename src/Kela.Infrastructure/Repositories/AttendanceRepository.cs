using Kela.Application.Features.Attendances;
using Kela.Domain.Entities;
using Kela.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kela.Infrastructure.Repositories;

internal sealed class AttendanceRepository(KelaDbContext context) : IAttendanceRepository
{
    public Task<Attendance?> GetAsync(int workspaceId, int studentId, DateOnly date, CancellationToken cancellationToken = default)
        => context.Attendances.FirstOrDefaultAsync(
            a => a.WorkspaceId == workspaceId && a.StudentId == studentId && a.Date == date, cancellationToken);

    public Task<List<Attendance>> GetDayAsync(int workspaceId, DateOnly date, CancellationToken cancellationToken = default)
        => context.Attendances
            .Where(a => a.WorkspaceId == workspaceId && a.Date == date)
            .OrderBy(a => a.StudentId)
            .ToListAsync(cancellationToken);

    public void Add(Attendance attendance) => context.Attendances.Add(attendance);

    public void Update(Attendance attendance) => context.Attendances.Update(attendance);

    public void Remove(Attendance attendance) => context.Attendances.Remove(attendance);
}
