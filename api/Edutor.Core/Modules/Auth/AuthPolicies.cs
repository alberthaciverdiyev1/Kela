namespace Edutor.Core.Modules.Auth;

public static class AuthRoles
{
    public const string Student = "student";
    public const string Teacher = "teacher";
    public const string Admin = "admin";
    public const string SuperAdmin = "superadmin";
}

public static class AuthPolicies
{
    public const string Student = "Student";

    /// <summary>İçerik idarəsi: teacher + admin + superadmin.</summary>
    public const string Teacher = "Teacher";

    /// <summary>İstifadəçi/tenant/parametr idarəsi: admin + superadmin.</summary>
    public const string Admin = "Admin";

    /// <summary>Yalnız superadmin.</summary>
    public const string SuperAdmin = "SuperAdmin";
}
