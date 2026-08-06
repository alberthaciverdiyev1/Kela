using Kela.Domain.Common;
using Kela.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Kela.Domain.Entities;

/// <summary>
/// Sistemin asıl kullanıcı aggregate'i. ASP.NET Core Identity ile bütünleşiktir:
/// parola hash'leme, parola doğrulama ve rol üyeliği Identity'nin
/// (UserManager/RoleManager/PasswordHasher) sorumluluğudur — burada elle hash yoktur.
/// Rol ↔ profil (Teacher/Student/Parent) tutarlılığı domain'de <see cref="AssignProfile"/>
/// ile garanti edilir: yalnızca role uyan tek profil kurulabilir.
/// </summary>
public class User : IdentityUser<int>, ISoftDeletable, IAuditableEntity
{
    public User(string firstName, string lastName, string email)
    {
        SetName(firstName, lastName);
        Email = email;
        UserName = email;
    }

    private User()
    {
        // EF Core materialization için
    }

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;

    public UserStatus Status { get; private set; } = UserStatus.Active;

    // 1:1 profil ilişkileri. Yalnızca role uyan biri dolu olabilir (AssignProfile garantisi).
    public Teacher? Teacher { get; private set; }
    public Student? Student { get; private set; }
    public Parent? Parent { get; private set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public void SetName(string firstName, string lastName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName, nameof(firstName));
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName, nameof(lastName));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetStatus(UserStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Role uyan tek profili kurar; diğer profilleri temizler.
    /// Böylece "Teacher iken Student profili" gibi tutarsız durum imkânsızdır.
    /// Admin rolünün profili yoktur. Identity rol üyeliği (AspNetUserRoles)
    /// service katmanında UserManager ile yapılır.
    /// </summary>
    public void AssignProfile(Role role)
    {
        Teacher = null;
        Student = null;
        Parent = null;

        var now = DateTime.UtcNow;
        Teacher = role == Role.Teacher ? new Teacher { User = this, CreatedAt = now } : null;
        Student = role == Role.Student ? new Student { User = this, CreatedAt = now } : null;
        Parent = role == Role.Parent ? new Parent { User = this, CreatedAt = now } : null;
    }
}
