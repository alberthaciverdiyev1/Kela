namespace Kela.Domain.Common;

public interface ISoftDeletable
{
    DateTime? DeletedAt { get; set; }
}

public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
}

// 1. ANA BASE ENTITY: Sistemin asıl omurgası. Farklı Id tipleri için açık kapı bırakır.
public abstract class BaseEntity<TId> : ISoftDeletable, IAuditableEntity
{
    public required TId Id { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}

// 2. DEFAULT BASE ENTITY: Senin "hepsi int zaten" ihtiyacın için kısayol.
// TId kısmına int vererek ana sınıftan miras alır.
public abstract class BaseEntity : BaseEntity<int>
{
    // Burası boş kalır. Sadece BaseEntity<int>'in bir kısayolu olarak çalışır.
}
