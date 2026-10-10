using LanguageLMS.Core.Common.Entities;

namespace LanguageLMS.Core.Modules.Auth.Entities;

public class Student : BaseEntity
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string PasswordHash { get; set; } = string.Empty;

    public string? Image { get; set; }

    public bool IsActive { get; set; } = true;

    public bool NotificationStatus { get; set; }
}
