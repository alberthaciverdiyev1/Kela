using LanguageLMS.Core.Common.Entities;

namespace LanguageLMS.Core.Modules.Notification.Entities;

public class NotificationToken : BaseEntity
{
    public long StudentId { get; set; }

    public string Token { get; set; } = string.Empty;

    /// <summary>ios | android | web</summary>
    public string? DeviceType { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset? LastUsedAt { get; set; }
}
