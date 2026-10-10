namespace LanguageLMS.Core.Common.Entities;

public abstract class BaseEntity
{
    public long Id { get; set; }

    public long TenantId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }
}
