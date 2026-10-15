using Kela.Core.Common.MultiTenancy;
using Kela.Core.Data;
using Kela.Core.Modules.Tenant.Requests;
using Kela.Core.Modules.Tenant.Responses;
using Microsoft.EntityFrameworkCore;

namespace Kela.Core.Modules.Tenant.Services;

using Tenant = Kela.Core.Modules.Tenant.Entities.Tenant;

public sealed class TenantService(AppDbContext db, ITenantProvider tenantProvider) : ITenantService
{
    public async Task<IReadOnlyList<TenantResponse>> ListAsync(CancellationToken ct = default)
        => await db.Tenants.AsNoTracking().OrderBy(x => x.Id)
            .Select(x => new TenantResponse(x.Id, x.Name, x.Slug, x.IsActive))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<TenantResponse>> ListPublicAsync(CancellationToken ct = default)
        => await db.Tenants.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name)
            .Select(x => new TenantResponse(x.Id, x.Name, x.Slug, x.IsActive))
            .ToListAsync(ct);

    public async Task<TenantResponse?> GetByIdAsync(long id, CancellationToken ct = default)
        => await db.Tenants.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new TenantResponse(x.Id, x.Name, x.Slug, x.IsActive))
            .FirstOrDefaultAsync(ct);

    public async Task<TenantResponse?> GetCurrentAsync(CancellationToken ct = default)
    {
        if (tenantProvider.TenantId is not long id) return null;

        return await db.Tenants.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new TenantResponse(x.Id, x.Name, x.Slug, x.IsActive))
            .FirstOrDefaultAsync(ct);
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

        return new TenantResponse(tenant.Id, tenant.Name, tenant.Slug, tenant.IsActive);
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
}
