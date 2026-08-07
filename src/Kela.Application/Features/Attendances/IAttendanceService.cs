using Kela.Application.Features.Attendances.Requests;
using Kela.Application.Features.Attendances.Responses;

namespace Kela.Application.Features.Attendances;

public interface IAttendanceService
{
    Task<AttendanceDayResponse> GetDayAsync(int workspaceId, DateOnly date, CancellationToken cancellationToken = default);
    Task SetMarksAsync(int workspaceId, DateOnly date, SetAttendanceMarksRequest request, CancellationToken cancellationToken = default);
    Task ClearMarkAsync(int workspaceId, int studentId, DateOnly date, CancellationToken cancellationToken = default);
}
