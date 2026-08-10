using Kela.Application.Features.Nodes.Responses;
using Kela.Domain.Entities;
using Kela.Domain.Enums;

namespace Kela.Application.Features.Nodes;

public static class NodeMappings
{
    public static ContentSummaryResponse ToSummary(this Content content) => new(
        content.Id,
        content.Title,
        content.Description,
        content.Type,
        content.Url,
        content.IsPublished,
        content.Lesson is null ? null : new LessonSummaryResponse(
            !string.IsNullOrEmpty(content.Lesson.VideoPath),
            content.Lesson.DurationSeconds));

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

    public static List<NodeResponse> FilterByType(this List<NodeResponse> nodes, ContentType type)
    {
        return nodes
            .Select(n => FilterNode(n, type))
            .Where(n => n is not null)
            .Cast<NodeResponse>()
            .ToList();
    }

    private static NodeResponse? FilterNode(NodeResponse node, ContentType type)
    {
        if (node.Kind == NodeType.Content)
        {
            return node.Content is not null && node.Content.Type == type ? node : null;
        }

        // Keep empty folders so users can build structure on a type-filtered page
        // (e.g. create a folder on the quizzes page before adding quizzes into it).
        if (node.Children.Count == 0)
        {
            return node;
        }

        var children = node.Children
            .Select(c => FilterNode(c, type))
            .Where(c => c is not null)
            .Cast<NodeResponse>()
            .ToList();

        return children.Count == 0 ? null : node with { Children = children };
    }
}
