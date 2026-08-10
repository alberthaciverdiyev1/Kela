using Kela.Web.Helpers;

namespace Kela.Web.Models.Workspaces;

public sealed record WorkspaceDataResponse(
    WorkspaceDetailResponse Workspace,
    IReadOnlyList<StudentResponse> AvailableStudents);
