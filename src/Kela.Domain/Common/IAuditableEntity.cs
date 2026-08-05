namespace Kela.Domain.Common;

/// <summary>
/// Kayıt yaşam döngüsü zaman damgalarını taşıyan entity'ler.
/// </summary>
public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
}
