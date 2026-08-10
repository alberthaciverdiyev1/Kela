using Kela.Application.Features.Nodes.Requests;
using Kela.Application.Features.Nodes.Responses;

namespace Kela.Application.Features.Nodes;

public interface INodeService
{
    Task<List<NodeResponse>> GetLibraryTreeAsync(int teacherId, CancellationToken cancellationToken = default);
    Task<List<NodeResponse>> GetWorkspaceTreeAsync(int workspaceId, CancellationToken cancellationToken = default);
    Task<int> CreateFolderAsync(CreateFolderRequest request, CancellationToken cancellationToken = default);
    Task<int> AddContentAsync(AddContentRequest request, CancellationToken cancellationToken = default);
    Task<int> CopyFolderAsync(CopyFolderRequest request, CancellationToken cancellationToken = default);
    Task UpdateNodeAsync(int id, UpdateNodeRequest request, CancellationToken cancellationToken = default);
    Task DeleteNodeAsync(int id, CancellationToken cancellationToken = default);
}
