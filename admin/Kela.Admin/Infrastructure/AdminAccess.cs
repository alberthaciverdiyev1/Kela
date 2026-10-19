using System.Security.Claims;
using Kela.Core.Modules.User.Entities;

namespace Kela.Admin.Infrastructure;

public static class AdminAccess
{
    /// <summary>Silinmiş qeydləri görmək/bərpa etmək yalnız admin və superadminin işidir.</summary>
    public static bool IsAdmin(this ClaimsPrincipal user) =>
        user.IsInRole(UserTypes.Admin) || user.IsInRole(UserTypes.SuperAdmin);

    public static long? CurrentUserId(this ClaimsPrincipal user) =>
        long.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;
}
