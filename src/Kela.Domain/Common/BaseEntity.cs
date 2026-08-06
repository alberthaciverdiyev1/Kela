namespace Kela.Domain.Common;

public abstract class BaseEntity<TId> : ISoftDeletable, IAuditableEntity
{
    public TId Id { get; set; } = default!;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}

public abstract class BaseEntity : BaseEntity<int>
{
}
