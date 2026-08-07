using Kela.Web.Helpers;

namespace Kela.Web.Models.Attendances;

public sealed record AttendancePageViewModel(
    int WorkspaceId,
    string WorkspaceName,
    int Year,
    int Month,
    IReadOnlyList<AttendanceStudentResponse> Students,
    IReadOnlyList<AttendanceRecordResponse> Records);

public sealed record SetAttendanceRequest(int StudentId, DateOnly Date, int Status);
