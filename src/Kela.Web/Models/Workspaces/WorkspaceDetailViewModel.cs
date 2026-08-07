using Kela.Web.Helpers;

namespace Kela.Web.Models.Workspaces;

public sealed record WorkspaceDetailViewModel(
    WorkspaceDetailResponse Workspace,
    IReadOnlyList<StudentResponse> AvailableStudents);
