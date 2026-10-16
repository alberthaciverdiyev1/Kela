using System.Security.Claims;
using Kela.Core.Modules.User.Entities;

namespace Kela.Mvc.Infrastructure;

public static class AdminAccess
{
    /// <summary>Silinmiş qeydləri görmək/bərpa etmək yalnız admin və superadminin işidir.</summary>
    public static bool IsAdmin(this ClaimsPrincipal user) =>
        user.IsInRole(UserTypes.Admin) || user.IsInRole(UserTypes.SuperAdmin);
}
