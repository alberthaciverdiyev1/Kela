namespace Kela.Domain.Common;

/// <summary>
/// Fiziksel silme yerine mantıksal silme (soft-delete) uygulanan entity'ler.
/// </summary>
public interface ISoftDeletable
{
    DateTime? DeletedAt { get; set; }
}
