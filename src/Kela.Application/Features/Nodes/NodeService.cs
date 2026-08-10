using FluentValidation;
using Kela.Application.Features.Contents;
using Kela.Application.Features.Nodes.Requests;
using Kela.Application.Features.Nodes.Responses;
using Kela.Application.Features.Workspaces;
using Kela.Application.Patterns;
using Kela.Domain.Entities;
using Kela.Domain.Enums;

namespace Kela.Application.Features.Nodes;

internal sealed class NodeService(
    INodeRepository nodes,
    IContentRepository contents,
    IWorkspaceRepository workspaces,
    IUnitOfWork unitOfWork,
    IValidator<CreateFolderRequest> createFolderValidator,
    IValidator<AddContentRequest> addContentValidator,
    IValidator<CopyFolderRequest> copyFolderValidator,
    IValidator<UpdateNodeRequest> updateNodeValidator) : INodeService
{
    public async Task<List<NodeResponse>> GetLibraryTreeAsync(int teacherId, ContentType? type = null, CancellationToken cancellationToken = default)
    {
        var items = await nodes.GetByContextAsync(null, teacherId, cancellationToken);
        var tree = items.ToTree();
        return type is null ? tree : tree.FilterByType(type.Value);
    }

    public async Task<List<NodeResponse>> GetWorkspaceTreeAsync(int workspaceId, CancellationToken cancellationToken = default)
    {
        var items = await nodes.GetByContextAsync(workspaceId, null, cancellationToken);
        return items.ToTree();
    }

    public async Task<int> CreateFolderAsync(CreateFolderRequest request, CancellationToken cancellationToken = default)
    {
        await createFolderValidator.ValidateAndThrowAsync(request, cancellationToken);

        var context = await nodes.GetByContextAsync(request.WorkspaceId, request.TeacherId, cancellationToken);
        NodeTree.EnsureParent(context, request.ParentId, request.WorkspaceId, request.TeacherId);

        var sibling = context.Where(n => n.ParentId == request.ParentId).ToList();
        var node = new Node
        {
            WorkspaceId = request.WorkspaceId,
            TeacherId = request.TeacherId,
            ParentId = request.ParentId,
            Name = request.Name.Trim(),
            Kind = NodeType.Folder,
            Position = NodeTree.NextPosition(sibling),
            CreatedAt = DateTime.UtcNow,
        };

        nodes.Add(node);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return node.Id;
    }

    public async Task<int> AddContentAsync(AddContentRequest request, CancellationToken cancellationToken = default)
    {
        await addContentValidator.ValidateAndThrowAsync(request, cancellationToken);

        var workspace = await workspaces.GetByIdAsync(request.WorkspaceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Id = {request.WorkspaceId} olan iş alanı bulunamadı.");

        var content = await contents.GetByIdAsync(request.ContentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Id = {request.ContentId} olan içerik bulunamadı.");

        if (workspace.TeacherId != content.TeacherId)
        {
            throw new InvalidOperationException("Bu içerik bu iş alanına ait değil.");
        }

        var context = await nodes.GetByContextAsync(request.WorkspaceId, null, cancellationToken);
        NodeTree.EnsureParent(context, request.ParentId, request.WorkspaceId, null);

        var sibling = context.Where(n => n.ParentId == request.ParentId).ToList();
        var node = new Node
        {
            WorkspaceId = request.WorkspaceId,
            ParentId = request.ParentId,
            Name = content.Title,
            Kind = NodeType.Content,
            ContentId = content.Id,
            Position = NodeTree.NextPosition(sibling),
            CreatedAt = DateTime.UtcNow,
        };

        nodes.Add(node);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return node.Id;
    }

    public async Task<int> CopyFolderAsync(CopyFolderRequest request, CancellationToken cancellationToken = default)
    {
        await copyFolderValidator.ValidateAndThrowAsync(request, cancellationToken);

        var workspace = await workspaces.GetByIdAsync(request.WorkspaceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Id = {request.WorkspaceId} olan iş alanı bulunamadı.");

        var source = await nodes.GetByIdAsync(request.SourceNodeId, cancellationToken)
            ?? throw new KeyNotFoundException($"Id = {request.SourceNodeId} olan düğüm bulunamadı.");

        if (source.Kind != NodeType.Folder || source.WorkspaceId is not null)
        {
            throw new InvalidOperationException("Yalnızca kütüphane klasörleri kopyalanabilir.");
        }

        if (source.TeacherId != workspace.TeacherId)
        {
            throw new InvalidOperationException("Bu klasör bu iş alanına ait değil.");
        }

        var libraryNodes = await nodes.GetByContextAsync(null, source.TeacherId, cancellationToken);
        var subtree = NodeTree.CollectSubtree(libraryNodes, source);

        var wsContext = await nodes.GetByContextAsync(request.WorkspaceId, null, cancellationToken);
        NodeTree.EnsureParent(wsContext, request.ParentId, request.WorkspaceId, null);

        var map = new Dictionary<int, Node>();
        Node? createdRoot = null;

        foreach (var n in subtree)
        {
            var copied = new Node
            {
                WorkspaceId = request.WorkspaceId,
                Name = n.Name,
                Kind = n.Kind,
                ContentId = n.ContentId,
                Position = n.Position,
                CreatedAt = DateTime.UtcNow,
            };

            if (n.Id == source.Id)
            {
                copied.ParentId = request.ParentId;
            }
            else
            {
                copied.Parent = map[n.ParentId!.Value];
            }

            nodes.Add(copied);
            map[n.Id] = copied;
            createdRoot ??= copied;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return createdRoot?.Id ?? throw new InvalidOperationException("Klasör kopyalanamadı.");
    }

    public async Task UpdateNodeAsync(int id, UpdateNodeRequest request, CancellationToken cancellationToken = default)
    {
        await updateNodeValidator.ValidateAndThrowAsync(request, cancellationToken);

        var node = await nodes.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Id = {id} olan düğüm bulunamadı.");

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            node.Name = request.Name.Trim();
        }

        if (request.Position is not null)
        {
            node.Position = request.Position.Value;
        }

        if (request.ParentId != node.ParentId && request.ParentId is not null)
        {
            if (request.ParentId == node.Id)
            {
                throw new InvalidOperationException("Bir düğüm kendi üstüne taşınamaz.");
            }

            var context = await nodes.GetByContextAsync(node.WorkspaceId, node.TeacherId, cancellationToken);
            NodeTree.EnsureParent(context, request.ParentId, node.WorkspaceId, node.TeacherId);

            if (NodeTree.DescendantIds(context, node.Id).Contains(request.ParentId.Value))
            {
                throw new InvalidOperationException("Bir klasör kendi alt klasörüne taşınamaz.");
            }

            node.ParentId = request.ParentId;
        }

        node.UpdatedAt = DateTime.UtcNow;
        nodes.Update(node);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteNodeAsync(int id, CancellationToken cancellationToken = default)
    {
        var node = await nodes.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Id = {id} olan düğüm bulunamadı.");

        var context = await nodes.GetByContextAsync(node.WorkspaceId, node.TeacherId, cancellationToken);
        var descendants = NodeTree.DescendantIds(context, node.Id);
        var now = DateTime.UtcNow;

        node.DeletedAt = now;

        foreach (var child in context.Where(n => descendants.Contains(n.Id)))
        {
            child.DeletedAt = now;
        }

        nodes.Update(node);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
