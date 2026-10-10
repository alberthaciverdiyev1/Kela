using LanguageLMS.Core.Data;
using LanguageLMS.Core.Modules.Tenant.Requests;
using LanguageLMS.Core.Modules.Tenant.Responses;
using Microsoft.EntityFrameworkCore;

namespace LanguageLMS.Core.Modules.Tenant.Services;

using Tenant = LanguageLMS.Core.Modules.Tenant.Entities.Tenant;

public sealed class TenantService(AppDbContext db) : ITenantService
{
    public async Task<IReadOnlyList<TenantResponse>> ListAsync(CancellationToken ct = default)
        => await db.Tenants.AsNoTracking().OrderBy(x => x.Id)
            .Select(x => new TenantResponse(x.Id, x.Name, x.Slug, x.IsActive))
            .ToListAsync(ct);

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
