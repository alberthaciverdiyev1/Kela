using Kela.Application.Features.Nodes;
using Kela.Domain.Entities;
using Kela.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kela.Infrastructure.Repositories;

internal sealed class NodeRepository(KelaDbContext context) : INodeRepository
{
    public async Task<List<Node>> GetByContextAsync(int? workspaceId, int? teacherId, CancellationToken cancellationToken = default)
    {
        var query = context.Nodes.Include(n => n.Content);

        if (workspaceId is not null)
        {
            return await query.Where(n => n.WorkspaceId == workspaceId)
                .OrderBy(n => n.Position)
                .ToListAsync(cancellationToken);
        }

        if (teacherId is not null)
        {
            return await query.Where(n => n.TeacherId == teacherId)
                .OrderBy(n => n.Position)
                .ToListAsync(cancellationToken);
        }

        return [];
    }

    public async Task<List<Node>> GetByContentIdAsync(int contentId, CancellationToken cancellationToken = default)
        => await context.Nodes.Where(n => n.ContentId == contentId).ToListAsync(cancellationToken);

    public async Task<Node?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await context.Nodes.Include(n => n.Content).FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

    public void Add(Node node) => context.Nodes.Add(node);

    public void Update(Node node) => context.Nodes.Update(node);

    public void Remove(Node node) => context.Nodes.Remove(node);
}
