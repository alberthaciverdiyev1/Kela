namespace Kela.Application.Features.Attendances.Requests;

public sealed record SetAttendanceMarksRequest(IReadOnlyList<AttendanceMarkRequest> Marks);
