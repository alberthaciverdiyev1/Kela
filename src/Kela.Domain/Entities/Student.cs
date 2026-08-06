using Kela.Domain.Common;

namespace Kela.Domain.Entities;

/// <summary>
/// Öğrenci profili. 1:1 olduğu için ayrı bir Id taşımaz —
/// primary key UserId'dir (shared primary key). Kendi kendine var olamaz.
/// </summary>
public class Student : ISoftDeletable, IAuditableEntity
{
    /// <summary>Primary key + FK → User.Id.</summary>
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<Section> Sections { get; set; } = new List<Section>();

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
