namespace Kela.Domain.Common;

/// <summary>
/// Sistem omurgası: tüm entity'lerin temelidir. Farklı Id tipleri için açık kapı bırakır.
/// </summary>
public abstract class BaseEntity<TId> : ISoftDeletable, IAuditableEntity
{
    public required TId Id { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}

/// <summary>
/// int kimlikli entity'ler için kısayol (projedeki tüm aggregate'ler int kullanır).
/// </summary>
public abstract class BaseEntity : BaseEntity<int>
{
}
