using Kela.Domain.Entities;

namespace Kela.Application.Features.Attendances;

public interface IAttendanceRepository
{
    Task<Attendance?> GetAsync(int workspaceId, int studentId, DateOnly date, CancellationToken cancellationToken = default);
    Task<List<Attendance>> GetDayAsync(int workspaceId, DateOnly date, CancellationToken cancellationToken = default);
    void Add(Attendance attendance);
    void Update(Attendance attendance);
    void Remove(Attendance attendance);
}
