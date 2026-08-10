using Kela.Domain.Common;
using Kela.Domain.Enums;

namespace Kela.Domain.Entities;

public class Content : BaseEntity
{
    public int TeacherId { get; set; }
    public User? Teacher { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ContentType Type { get; set; }
    public string? Url { get; set; }
    public bool IsPublished { get; set; }

    public ICollection<Node> Nodes { get; set; } = new List<Node>();
}
