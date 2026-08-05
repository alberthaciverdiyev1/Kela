using Kela.Domain.Common;
using Kela.Domain.Users.Enums;

namespace Kela.Domain.Users;

public class User : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;
    public Role Role { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;

    // 1:1 profile relationships. Only the one matching Role should be populated.
    public Teacher? Teacher { get; set; }
    public Student? Student { get; set; }
    public Parent? Parent { get; set; }
}
