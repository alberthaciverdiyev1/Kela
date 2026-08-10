using Kela.Domain.Entities;

namespace Kela.Application.Features.Nodes;

internal static class NodeTree
{
    public static int NextPosition(IEnumerable<Node> siblings)
        => siblings.Any() ? siblings.Max(n => n.Position) + 1 : 1;

    public static void EnsureParent(List<Node> context, int? parentId, int? workspaceId, int? teacherId)
    {
        if (parentId is null)
        {
            return;
        }

        var parent = context.FirstOrDefault(n => n.Id == parentId)
            ?? throw new KeyNotFoundException("Üst klasör bulunamadı.");

        if (parent.WorkspaceId != workspaceId || parent.TeacherId != teacherId)
        {
            throw new InvalidOperationException("Üst klasör bu bağlama ait değil.");
        }
    }

    public static HashSet<int> DescendantIds(List<Node> all, int rootId)
    {
        var result = new HashSet<int>();

        void Walk(int id)
        {
            foreach (var child in all.Where(n => n.ParentId == id))
            {
                if (result.Add(child.Id))
                {
                    Walk(child.Id);
                }
            }
        }

        Walk(rootId);
        return result;
    }

    public static List<Node> CollectSubtree(List<Node> all, Node root)
    {
        var result = new List<Node>();

        void Walk(Node node)
        {
            result.Add(node);
            foreach (var child in all.Where(n => n.ParentId == node.Id).OrderBy(n => n.Position))
            {
                Walk(child);
            }
        }

        Walk(root);
        return result;
    }
}
