using Kela.Domain.Common;
using Kela.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Kela.Domain.Entities;

/// <summary>
/// Sistemin asıl kullanıcı aggregate'i. ASP.NET Core Identity ile bütünleşiktir:
/// parola hash'leme, parola doğrulama ve rol üyeliği Identity'nin
/// (UserManager/RoleManager/PasswordHasher) sorumluluğudur — burada elle hash yoktur.
/// Kullanıcının rolü yalnızca Identity rol üyeliğinden (AspNetUserRoles) okunur;
/// role göre ayrı profil tablosu yoktur.
/// </summary>
public class User : IdentityUser<int>, ISoftDeletable, IAuditableEntity
{
    public User(string firstName, string lastName, string email)
    {
        SetName(firstName, lastName);
        Email = email;
        UserName = email;
    }

    public User(string firstName, string lastName, string email, string phoneNumber)
        : this(firstName, lastName, email)
    {
        SetPhoneNumber(phoneNumber);
    }

    private User()
    {
        // EF Core materialization için
    }

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    // PhoneNumber bilinçli olarak yeniden bildirilmez: IdentityUser<int> zaten
    // public get/set verir ve AspNetUsers.PhoneNumber kolonuna eşlenir. Burada
    // bir hiding/override değil, doğrudan base property kullanılır — böylece
    // EF'in map ettiği alan ile UserManager'ın okuduğu alan aynıdır.

    public UserStatus Status { get; private set; } = UserStatus.Active;

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

    public void SetPhoneNumber(string? phoneNumber)
    {
        PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetStatus(UserStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }
}
