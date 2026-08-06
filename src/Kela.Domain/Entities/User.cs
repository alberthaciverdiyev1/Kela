using Kela.Domain.Common;
using Kela.Domain.Enums;

namespace Kela.Domain.Entities;

/// <summary>
/// Sistemin asıl kullanıcı aggregate'i.
/// Rol ↔ profil (Teacher/Student/Parent) tutarlılığı domain'de <see cref="AssignProfile"/>
/// ile garanti edilir: yalnızca Role'a uyan tek profil kurulabilir.
/// </summary>
public class User : BaseEntity
{
    public User(string firstName, string lastName, string email)
    {
        SetName(firstName, lastName);
        Email = email;
    }

    private User()
    {
        // EF Core materialization için
    }

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Parola hash'idir; düz metin parola asla saklanmaz. Hash dışarıdan
    /// doğrudan set edilemez — <see cref="SetPasswordHash"/> ile verilir.
    /// </summary>
    public string PasswordHash { get; private set; } = string.Empty;

    public string PhoneNumber { get; private set; } = string.Empty;
    public Role Role { get; private set; }
    public UserStatus Status { get; private set; } = UserStatus.Active;

    // 1:1 profil ilişkileri. Yalnızca Role'a uyan biri dolu olabilir (AssignProfile garantisi).
    public Teacher? Teacher { get; private set; }
    public Student? Student { get; private set; }
    public Parent? Parent { get; private set; }

    public void SetName(string firstName, string lastName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName, nameof(firstName));
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName, nameof(lastName));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPhoneNumber(string phoneNumber)
    {
        PhoneNumber = phoneNumber?.Trim() ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Hash'lenmiş parolayı atar. Hash'lemeyi Application katmanı yapar (IPasswordHasher).</summary>
    public void SetPasswordHash(string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash, nameof(passwordHash));
        PasswordHash = passwordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetStatus(UserStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Rol atar ve yalnızca o role uyan profili kurar; diğer profilleri temizler.
    /// Böylece "Role=Teacher iken Student profili" gibi tutarsız durum imkânsızdır.
    /// Admin rolünün profili yoktur.
    /// </summary>
    public void AssignProfile(Role role)
    {
        Role = role;
        Teacher = null;
        Student = null;
        Parent = null;

        Teacher = role == Role.Teacher ? new Teacher { User = this } : null;
        Student = role == Role.Student ? new Student { User = this } : null;
        Parent = role == Role.Parent ? new Parent { User = this } : null;
    }
}
