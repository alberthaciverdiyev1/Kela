using Kela.Core.Common.Entities;

namespace Kela.Core.Modules.Auth.Entities;

public class Student : BaseEntity
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string PasswordHash { get; set; } = string.Empty;

    public string? Image { get; set; }

    public bool IsActive { get; set; } = true;

    public bool NotificationStatus { get; set; }

    /// <summary>İlk girişdə şifrəni dəyişmək məcburiyyəti (təsadüfi şifrə ilə yaradılan şagirdlər).</summary>
    public bool MustChangePassword { get; set; }

    /// <summary>Admin tərəfindən generasiya olunmuş şagird ilk girişdə ad-soyadını yazmalıdır.</summary>
    public bool MustChangeName { get; set; }
}
