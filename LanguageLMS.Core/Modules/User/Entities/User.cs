using LanguageLMS.Core.Common.Entities;

namespace LanguageLMS.Core.Modules.User.Entities;

public class User : BaseEntity
{
    public string? Name { get; set; }

    public string? Surname { get; set; }

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string Type { get; set; } = UserTypes.Teacher;
}

public static class UserTypes
{
    public const string Admin = "admin";
    public const string Teacher = "teacher";

    public static bool IsValid(string type) => type is Admin or Teacher;
}
