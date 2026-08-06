namespace Kela.Domain.Common;

public static class RoleNames
{
    public const string Admin   = "Admin";
    public const string Teacher = "Teacher";
    public const string Student = "Student";
    public const string Parent  = "Parent";

    public static readonly IReadOnlyList<string> All = new[] { Admin, Teacher, Student, Parent };

    public static bool IsValid(string? roleName)
        => !string.IsNullOrWhiteSpace(roleName) && All.Contains(roleName);
}
