using Kela.Core.Common.MultiTenancy;
using Kela.Core.Common.Storage;
using Kela.Core.Data;
using Kela.Core.Modules.Tenant.Requests;
using Kela.Core.Modules.Tenant.Responses;
using Microsoft.EntityFrameworkCore;

namespace Kela.Core.Modules.Tenant.Services;

using Tenant = Kela.Core.Modules.Tenant.Entities.Tenant;

public sealed class TenantService(AppDbContext db, ITenantProvider tenantProvider, IFileStorage storage) : ITenantService
{
    public async Task<IReadOnlyList<TenantResponse>> ListAsync(CancellationToken ct = default)
    {
        var tenants = await db.Tenants.AsNoTracking().OrderBy(x => x.Id)
            .Select(x => new { x.Id, x.Name, x.Slug, x.IsActive })
            .ToListAsync(ct);

        var logos = await LogoMapAsync(ct);
        return tenants.Select(t => new TenantResponse(t.Id, t.Name, t.Slug, t.IsActive, Lookup(logos, t.Id))).ToList();
    }

    public async Task<IReadOnlyList<TenantResponse>> ListPublicAsync(CancellationToken ct = default)
    {
        var tenants = await db.Tenants.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name, x.Slug, x.IsActive })
            .ToListAsync(ct);

        var logos = await LogoMapAsync(ct);
        return tenants.Select(t => new TenantResponse(t.Id, t.Name, t.Slug, t.IsActive, Lookup(logos, t.Id))).ToList();
    }

    public async Task<TenantResponse?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var t = await db.Tenants.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new { x.Id, x.Name, x.Slug, x.IsActive })
            .FirstOrDefaultAsync(ct);

        if (t is null) return null;
        return new TenantResponse(t.Id, t.Name, t.Slug, t.IsActive, await LogoAsync(t.Id, ct));
    }

    public async Task<TenantResponse?> GetCurrentAsync(CancellationToken ct = default)
    {
        if (tenantProvider.TenantId is not long id) return null;

        var t = await db.Tenants.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new { x.Id, x.Name, x.Slug, x.IsActive })
            .FirstOrDefaultAsync(ct);

        if (t is null) return null;
        return new TenantResponse(t.Id, t.Name, t.Slug, t.IsActive, await LogoAsync(t.Id, ct));
    }

    public async Task<TenantResponse> UpdateCurrentAsync(UpdateTenantProfileRequest request, CancellationToken ct = default)
    {
        if (tenantProvider.TenantId is not long id)
        {
            throw new InvalidOperationException("Tenant context is required");
        }

        var tenant = await db.Tenants.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Tenant not found: {id}");

        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required");
        }

        tenant.Name = name;
        await db.SaveChangesAsync(ct);

        return new TenantResponse(tenant.Id, tenant.Name, tenant.Slug, tenant.IsActive, await LogoAsync(tenant.Id, ct));
    }

    public async Task<TenantResponse> CreateAsync(CreateTenantRequest request, CancellationToken ct = default)
    {
        var slug = request.Slug.Trim().ToLowerInvariant();

        if (await db.Tenants.AnyAsync(x => x.Slug == slug, ct))
        {
            throw new ArgumentException("A tenant with this slug already exists");
        }

        var tenant = new Tenant { Name = request.Name, Slug = slug };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync(ct);

        return new TenantResponse(tenant.Id, tenant.Name, tenant.Slug, tenant.IsActive);
    }

    private async Task<Dictionary<long, string>> LogoMapAsync(CancellationToken ct)
    {
        var rows = await db.Settings.IgnoreQueryFilters(new[] { "Tenant" }).AsNoTracking()
            .Where(s => s.Logo != null && s.TenantId != 0)
            .Select(s => new { s.TenantId, s.Logo })
            .ToListAsync(ct);

        return rows.ToDictionary(r => r.TenantId, r => storage.GetUrl(r.Logo));
    }

    private async Task<string?> LogoAsync(long tenantId, CancellationToken ct)
    {
        var logo = await db.Settings.IgnoreQueryFilters(new[] { "Tenant" }).AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .Select(s => s.Logo)
            .FirstOrDefaultAsync(ct);

        return storage.GetUrl(logo);
    }

    private static string? Lookup(Dictionary<long, string> map, long id) => map.TryGetValue(id, out var url) ? url : null;
}
