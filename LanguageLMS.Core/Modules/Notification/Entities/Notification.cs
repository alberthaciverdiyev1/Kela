using LanguageLMS.Core.Common.Entities;

namespace LanguageLMS.Core.Modules.Notification.Entities;

public class Notification : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    public string? Image { get; set; }

    public string? Body { get; set; }

    /// <summary>true = bütün öğrencilere; false = seçilmiş öğrencilere.</summary>
    public bool All { get; set; } = true;

    public string? Data { get; set; }

    public long? UserId { get; set; }

    public ICollection<NotificationStudent> Recipients { get; set; } = new List<NotificationStudent>();
}
