using Kela.Application.Abstractions.Security;
using Kela.Application.Abstractions.Tenancy;
using Kela.Domain.Tenants.Enums;
using Kela.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kela.Api.Middleware;

/// <summary>
/// İsteğin tenant'ını çözer ve hem ICurrentTenant'a hem KelaDbContext'e uygular.
/// Öncelik sırası:
///   0. Authenticated ise "tenant_id" cookie claim'i (login'de yazılan imzalı claim)
///   1. X-Tenant-Id   header (anonim; int id, DB'den doğrulanır)
///   2. X-Tenant-Slug header (anonim; string slug, DB'den doğrulanır)
///   3. Subdomain     (anonim; örn. okul1.kela.com)
/// Authenticated isteklerde tenant YALNIZCA cookie claim'inden gelir — header/subdomain
/// fallback uygulanmaz (tenant spoofing'i engellenir). Tenant çözümlenemezse ve
/// Tenancy:EnforceTenant=true ise 400 döner (fail-closed).
/// </summary>
public sealed class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly bool _enforceTenant;
    private readonly string[] _rootDomains;

    public TenantResolutionMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _enforceTenant = configuration.GetValue<bool>("Tenancy:EnforceTenant");
        _rootDomains = configuration.GetSection("Tenancy:RootDomains").Get<string[]>() ?? [];
    }

    public async Task InvokeAsync(HttpContext context, ICurrentTenant currentTenant, KelaDbContext db)
    {
        var tenantId = await ResolveTenantIdAsync(context, db);

        if (tenantId.HasValue)
        {
            db.SetTenantId(tenantId.Value);
            currentTenant.SetTenant(tenantId.Value);
        }
        else if (_enforceTenant)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Tenant çözümlenemedi. 'X-Tenant-Id' veya 'X-Tenant-Slug' header'ı gönderin ya da tenant subdomain'ini kullanın."
            });
            return;
        }

        await _next(context);
    }

    private async Task<int?> ResolveTenantIdAsync(HttpContext context, KelaDbContext db)
    {
        // 0) Authenticated → cookie claim'i (login'de yazılan imzalı "tenant_id").
        //    Claim yoksa / aktif değilse header'a düşmeyiz — authenticated istekte
        //    tenant yalnızca cookie'den gelir (spoofing engeli).
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var claim = context.User.FindFirst(AuthClaimTypes.TenantId);
            if (claim is not null
                && int.TryParse(claim.Value, out var claimedId)
                && await IsTenantActiveAsync(db, claimedId))
            {
                return claimedId;
            }

            return null;
        }

        // 1) X-Tenant-Id header (anonim)
        if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var idValue)
            && int.TryParse(idValue, out var id)
            && await IsTenantActiveAsync(db, id))
        {
            return id;
        }

        // 2) X-Tenant-Slug header (anonim)
        if (context.Request.Headers.TryGetValue("X-Tenant-Slug", out var slugValue)
            && !string.IsNullOrWhiteSpace(slugValue))
        {
            var tenantId = await FindTenantIdBySlugAsync(db, slugValue.ToString());
            if (tenantId.HasValue)
            {
                return tenantId;
            }
        }

        // 3) Subdomain (anonim; örn. default.localhost, okul1.kela.com)
        var host = context.Request.Host.Host;
        if (!string.IsNullOrWhiteSpace(host))
        {
            foreach (var root in _rootDomains)
            {
                var suffix = "." + root;
                if (host.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    var subdomain = host[..^suffix.Length];
                    var tenantId = await FindTenantIdBySlugAsync(db, subdomain);
                    if (tenantId.HasValue)
                    {
                        return tenantId;
                    }
                }
            }
        }

        return null;
    }

    private Task<bool> IsTenantActiveAsync(KelaDbContext db, int id)
        => db.Tenants.AnyAsync(t => t.Id == id && t.Status == TenantStatus.Active);

    private async Task<int?> FindTenantIdBySlugAsync(KelaDbContext db, string slug)
        => await db.Tenants
            .Where(t => t.Slug == slug && t.Status == TenantStatus.Active)
            .Select(t => (int?)t.Id)
            .FirstOrDefaultAsync();
}
