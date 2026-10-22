using System.Security.Claims;
using Kela.Core.Modules.User.Entities;

namespace Kela.Admin.Infrastructure;

public static class AdminAccess
{
    /// <summary>Silinmiş qeydləri görmək/bərpa etmək yalnız admin və superadminin işidir.</summary>
    public static bool IsAdmin(this ClaimsPrincipal user) =>
        user.IsInRole(UserTypes.Admin) || user.IsInRole(UserTypes.SuperAdmin);

    /// <summary>Cookie-dəki tenant_id claim-i. Superadmin tenant seçməyibsə 0.</summary>
    public static long CurrentTenantId(this ClaimsPrincipal user) =>
        long.TryParse(user.FindFirst("tenant_id")?.Value, out var id) ? id : 0;

    public static long? CurrentUserId(this ClaimsPrincipal user) =>
        long.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;
}
