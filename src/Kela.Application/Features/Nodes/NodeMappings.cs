using Kela.Application.Features.Nodes.Responses;
using Kela.Domain.Entities;

namespace Kela.Application.Features.Nodes;

public static class NodeMappings
{
    public static ContentSummaryResponse ToSummary(this Content content) => new(
        content.Id,
        content.Title,
        content.Description,
        content.Type,
        content.Url,
        content.IsPublished);

    public static NodeResponse ToResponse(this Node node, IReadOnlyList<NodeResponse> children) => new(
        node.Id,
        node.Name,
        node.Kind,
        node.Position,
        node.ParentId,
        node.ContentId,
        node.Content?.ToSummary(),
        children);

    public static List<NodeResponse> ToTree(this List<Node> nodes)
    {
        var childMap = nodes
            .Where(n => n.ParentId is not null)
            .GroupBy(n => n.ParentId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderBy(n => n.Position).ToList());

        NodeResponse Build(Node node)
        {
            var children = childMap.TryGetValue(node.Id, out var kids)
                ? kids.Select(Build).ToList()
                : new List<NodeResponse>();
            return node.ToResponse(children);
        }

        return nodes
            .Where(n => n.ParentId is null)
            .OrderBy(n => n.Position)
            .Select(Build)
            .ToList();
    }
}
