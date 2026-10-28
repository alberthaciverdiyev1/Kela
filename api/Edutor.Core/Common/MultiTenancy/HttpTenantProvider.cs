using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Edutor.Core.Common.MultiTenancy;

/// <summary>
/// Tenant-ı bu qaydada seçir:
/// - Autentifikasiyalı adi istifadəçi (student/teacher/admin): yalnız token-dəki "tenant_id" claim-i.
///   (X-Tenant-Id header-ına etibar edilmir — cross-tenant sızmasının qarşısı alınır.)
/// - Superadmin: X-Tenant-Id header-ı ilə tenant seçə bilər; seçməzsə cross-tenant rejim.
/// - Anonim (login/register/tenant siyahısı): ITenantScope.Override → X-Tenant-Id header.
/// </summary>
public sealed class HttpTenantProvider(IHttpContextAccessor accessor, ITenantScope scope) : ITenantProvider
{
    public long? TenantId
    {
        get
        {
            var context = accessor.HttpContext;
            // HTTP konteksti olmayan yer (arxa plan işi) → açıq şəkildə verilən tenant.
            if (context is null) return scope.Override;

            var isAuthenticated = context.User?.Identity?.IsAuthenticated == true;
            var isSuperAdmin = GetRole(context) == "superadmin";

            // Autentifikasiyalı adi istifadəçi → yalnız claim.
            if (isAuthenticated && !isSuperAdmin)
            {
                return ClaimTenant(context);
            }

            // Anonim və ya superadmin → override/header/claim sırası ilə.
            if (scope.Override is long overrideId) return overrideId;

            var header = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();
            if (long.TryParse(header, out var headerId) && headerId > 0) return headerId;

            return ClaimTenant(context);
        }
    }

    public bool IgnoreTenant
    {
        get
        {
            var context = accessor.HttpContext;
            if (context?.User?.Identity?.IsAuthenticated != true) return false;
            if (GetRole(context) != "superadmin") return false;
            if (scope.Override is not null) return false;
            if (context.Request.Headers.ContainsKey("X-Tenant-Id")) return false;

            // Superadmin bir tenant-a "switch" edibsə (tenant_id claim var), cross-tenant deyil.
            if (ClaimTenant(context) is not null) return false;

            return true;
        }
    }

    private static string? GetRole(HttpContext context) =>
        context.User?.FindFirst(ClaimTypes.Role)?.Value
        ?? context.User?.FindFirst("role")?.Value;

    private static long? ClaimTenant(HttpContext context)
    {
        var claim = context.User?.FindFirst("tenant_id")?.Value;
        return long.TryParse(claim, out var id) && id > 0 ? id : null;
    }
}
