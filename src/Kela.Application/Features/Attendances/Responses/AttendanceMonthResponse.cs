namespace Kela.Application.Features.Attendances.Responses;

public sealed record AttendanceMonthResponse(
    int WorkspaceId,
    int Year,
    int Month,
    IReadOnlyList<AttendanceStudentResponse> Students,
    IReadOnlyList<AttendanceRecordResponse> Records);
