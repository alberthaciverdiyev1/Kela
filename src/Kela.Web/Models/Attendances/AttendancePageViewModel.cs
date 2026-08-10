namespace Kela.Web.Models.Attendances;

public sealed record WorkspaceOption(int Id, string Name);

public sealed record AttendancePageViewModel(
    int WorkspaceId,
    string WorkspaceName,
    int Year,
    int Month,
    IReadOnlyList<WorkspaceOption> Workspaces);

public sealed record SetAttendanceRequest(int WorkspaceId, int StudentId, DateOnly Date, int Status);
