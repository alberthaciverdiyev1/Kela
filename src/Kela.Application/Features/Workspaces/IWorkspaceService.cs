using Kela.Application.Features.Workspaces.Requests;
using Kela.Application.Features.Workspaces.Responses;
using Kela.Application.Pagination;

namespace Kela.Application.Features.Workspaces;

public interface IWorkspaceService
{
    Task<PaginatedResult<WorkspaceResponse>> GetPageAsync(int teacherId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<WorkspaceDetailResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(CreateWorkspaceRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, UpdateWorkspaceRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task AddStudentsAsync(int id, AddStudentsRequest request, CancellationToken cancellationToken = default);
    Task RemoveStudentAsync(int id, int studentId, CancellationToken cancellationToken = default);
}
