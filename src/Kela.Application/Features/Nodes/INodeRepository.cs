using Kela.Domain.Entities;

namespace Kela.Application.Features.Nodes;

public interface INodeRepository
{
    Task<List<Node>> GetByContextAsync(int? workspaceId, int? teacherId, CancellationToken cancellationToken = default);
    Task<List<Node>> GetByContentIdAsync(int contentId, CancellationToken cancellationToken = default);
    Task<Node?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    void Add(Node node);
    void Update(Node node);
    void Remove(Node node);
}
