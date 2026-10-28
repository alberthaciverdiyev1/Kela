using Kela.Core.Common.Entities;

namespace Kela.Core.Modules.User.Entities;

public class User : BaseEntity
{
    public string? Name { get; set; }

    public string? Surname { get; set; }

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string Type { get; set; } = UserTypes.Teacher;

    /// <summary>İstifadəçinin sidebar-da gizlətdiyi menyu açarları.</summary>
    public List<string> HiddenSidebarItems { get; set; } = [];
}

public static class UserTypes
{
    public const string Teacher = "teacher";
    public const string Admin = "admin";
    public const string SuperAdmin = "superadmin";

    public static bool IsValid(string type) => type is Teacher or Admin or SuperAdmin;
}
