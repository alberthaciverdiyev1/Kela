namespace Kela.Web.Helpers;

public static class AppConstants
{
    public const string ApiAuthCookie = "Kela.Auth";
    public const string WebAuthCookie = "Kela.Lms.Auth";
    public const string LangCookie = "kela.lang";
    public const string DefaultLang = "az";

    public static readonly string[] Langs = ["az", "en", "ru", "tr"];

    public const string RoleAdmin = "Admin";
    public const string RoleTeacher = "Teacher";
    public const string RoleStudent = "Student";
    public const string RoleParent = "Parent";

    public static string HomeRouteFor(string role) => role switch
    {
        RoleTeacher => "/teacher/dashboard",
        RoleStudent => "/student/dashboard",
        RoleParent => "/parent/dashboard",
        _ => "/blocked",
    };
}
