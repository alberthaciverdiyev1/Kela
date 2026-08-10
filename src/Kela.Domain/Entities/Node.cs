using Kela.Domain.Common;
using Kela.Domain.Enums;

namespace Kela.Domain.Entities;

public class Node : BaseEntity
{
    public int? WorkspaceId { get; set; }
    public Workspace? Workspace { get; set; }

    public int? TeacherId { get; set; }
    public User? Teacher { get; set; }

    public int? ParentId { get; set; }
    public Node? Parent { get; set; }

    public ICollection<Node> Children { get; set; }
        = new List<Node>();

    public string Name { get; set; } = string.Empty;
    public NodeType Kind { get; set; }
    public int Position { get; set; }

    public int? ContentId { get; set; }
    public Content? Content { get; set; }

}
