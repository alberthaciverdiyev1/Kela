using LanguageLMS.Core.Common.Entities;

namespace LanguageLMS.Core.Modules.Notification.Entities;

public class NotificationStudent : BaseEntity
{
    public long NotificationId { get; set; }

    public Notification? Notification { get; set; }

    public long StudentId { get; set; }
}
