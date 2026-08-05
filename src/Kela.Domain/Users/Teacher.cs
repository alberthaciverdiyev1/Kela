using Kela.Domain.Common;
using Kela.Domain.Subjects;

namespace Kela.Domain.Users;

/// <summary>
/// Öğretmen profili. 1:1 olduğu için ayrı bir Id taşımaz —
/// primary key UserId'dir (shared primary key). Kendi kendine var olamaz.
/// </summary>
public class Teacher : ITenantEntity, ISoftDeletable, IAuditableEntity
{
    public int TenantId { get; set; }

    /// <summary>Primary key + FK → User.Id.</summary>
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<Subject> Subjects { get; set; } = new List<Subject>();

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
