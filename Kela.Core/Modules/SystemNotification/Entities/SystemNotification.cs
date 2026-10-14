using Kela.Core.Common.Entities;

namespace Kela.Core.Modules.SystemNotification.Entities;

public class SystemNotification : BaseEntity
{
    public long RecipientId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Body { get; set; }

    public string? Data { get; set; }

    public DateTimeOffset? ReadAt { get; set; }
}
