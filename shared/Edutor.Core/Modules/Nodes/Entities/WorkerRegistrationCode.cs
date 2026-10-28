using Edutor.Core.Common.Entities;

namespace Edutor.Core.Modules.Nodes.Entities;

/// <summary>
/// Admin paneldən yaradılan birdəfəlik qeydiyyat kodu. Cihaz ilk dəfə bu kodla
/// qoşulur və öz access token-ini alır.
/// </summary>
public class WorkerRegistrationCode : BaseEntity
{
    public string Code { get; set; } = string.Empty;

    public string? Note { get; set; }

    public DateTimeOffset? ExpiresAt { get; set; }

    public DateTimeOffset? UsedAt { get; set; }

    public long? UsedByNodeId { get; set; }

    public bool IsUsed => UsedAt is not null;
}
