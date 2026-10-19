using System.Text;
using Kela.Core.Common.Localization;
using Kela.Core.Common.MultiTenancy;
using Kela.Core.Common.Storage;
using Kela.Core.Data;
using Kela.Core.Modules.Tenant.Requests;
using Kela.Core.Modules.Tenant.Responses;
using Kela.Core.Modules.User.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Kela.Core.Modules.Tenant.Services;

using Tenant = Kela.Core.Modules.Tenant.Entities.Tenant;
using User = Kela.Core.Modules.User.Entities.User;

public sealed class TenantService(
    AppDbContext db,
    ITenantProvider tenantProvider,
    IFileStorage storage,
    IStringLocalizer<SharedResource> L) : ITenantService
{
    public async Task<IReadOnlyList<TenantResponse>> ListAsync(CancellationToken ct = default)
    {
        var tenants = await db.Tenants.AsNoTracking().OrderBy(x => x.Id)
            .Select(x => new { x.Id, x.Name, x.Slug, x.IsActive, x.IsPending })
            .ToListAsync(ct);

        var logos = await LogoMapAsync(ct);
        return tenants.Select(t => new TenantResponse(t.Id, t.Name, t.Slug, t.IsActive, Lookup(logos, t.Id), t.IsPending)).ToList();
    }

    public async Task<IReadOnlyList<TenantResponse>> ListPublicAsync(CancellationToken ct = default)
    {
        var tenants = await db.Tenants.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name, x.Slug, x.IsActive, x.IsPending })
            .ToListAsync(ct);

        var logos = await LogoMapAsync(ct);
        return tenants.Select(t => new TenantResponse(t.Id, t.Name, t.Slug, t.IsActive, Lookup(logos, t.Id), t.IsPending)).ToList();
    }

    public async Task<TenantResponse?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var t = await db.Tenants.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new { x.Id, x.Name, x.Slug, x.IsActive, x.IsPending })
            .FirstOrDefaultAsync(ct);

        if (t is null) return null;
        return new TenantResponse(t.Id, t.Name, t.Slug, t.IsActive, await LogoAsync(t.Id, ct), t.IsPending);
    }

    public async Task<TenantResponse?> GetCurrentAsync(CancellationToken ct = default)
    {
        if (tenantProvider.TenantId is not long id) return null;

        var t = await db.Tenants.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new { x.Id, x.Name, x.Slug, x.IsActive, x.IsPending })
            .FirstOrDefaultAsync(ct);

        if (t is null) return null;
        return new TenantResponse(t.Id, t.Name, t.Slug, t.IsActive, await LogoAsync(t.Id, ct), t.IsPending);
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

        return new TenantResponse(tenant.Id, tenant.Name, tenant.Slug, tenant.IsActive, await LogoAsync(tenant.Id, ct), tenant.IsPending);
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

        return new TenantResponse(tenant.Id, tenant.Name, tenant.Slug, tenant.IsActive, IsPending: tenant.IsPending);
    }

    public async Task<TenantResponse> RegisterAsync(RegisterTenantRequest request, CancellationToken ct = default)
    {
        var courseName = request.CourseName.Trim();
        if (courseName.Length == 0) throw new ArgumentException(L["Tenant_CourseNameRequired"]);
        if (courseName.Length > 190) courseName = courseName[..190];

        var slug = NormalizeSlug(string.IsNullOrWhiteSpace(request.Slug) ? courseName : request.Slug);
        if (slug.Length == 0) throw new ArgumentException(L["Tenant_CourseNameRequired"]);
        if (slug.Length > 100) slug = slug[..100];

        var adminName = request.AdminName.Trim();
        if (adminName.Length == 0) throw new ArgumentException(L["Tenant_AdminNameRequired"]);

        var email = request.Email.Trim().ToLowerInvariant();
        if (email.Length == 0 || !email.Contains('@') || email.StartsWith('@') || email.EndsWith('@'))
        {
            throw new ArgumentException(L["Validation_EmailInvalid"]);
        }

        if (request.Password.Length < 8
            || !request.Password.Any(char.IsLetter)
            || !request.Password.Any(char.IsDigit))
        {
            throw new ArgumentException(L["Tenant_PasswordRequirements"]);
        }

        if (request.Password != request.ConfirmPassword)
        {
            throw new ArgumentException(L["Tenant_PasswordMismatch"]);
        }

        if (!request.AcceptTerms)
        {
            throw new ArgumentException(L["Tenant_TermsRequired"]);
        }

        if (await db.Tenants.AnyAsync(x => x.Slug == slug, ct))
        {
            throw new ArgumentException(L["Tenant_SlugTaken"]);
        }

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var tenant = new Tenant { Name = courseName, Slug = slug, IsActive = false, IsPending = true };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync(ct);

        db.Users.Add(new User
        {
            TenantId = tenant.Id,
            Name = adminName,
            Surname = request.AdminSurname.Trim(),
            Email = email,
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Type = UserTypes.Admin
        });
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return new TenantResponse(tenant.Id, tenant.Name, tenant.Slug, tenant.IsActive, IsPending: tenant.IsPending);
    }

    public async Task<TenantResponse> SetActiveAsync(long id, bool isActive, CancellationToken ct = default)
    {
        var tenant = await db.Tenants.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Tenant not found: {id}");

        tenant.IsActive = isActive;
        if (isActive) tenant.IsPending = false;
        await db.SaveChangesAsync(ct);

        return new TenantResponse(tenant.Id, tenant.Name, tenant.Slug, tenant.IsActive, await LogoAsync(tenant.Id, ct), tenant.IsPending);
    }

    public Task<int> CountPendingAsync(CancellationToken ct = default)
        => db.Tenants.CountAsync(x => x.IsPending, ct);

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var tenant = await db.Tenants.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Tenant not found: {id}");

        if (tenant.IsActive)
        {
            throw new ArgumentException("An active tenant cannot be deleted");
        }

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        // Müraciət heç bir real məlumat yaratmayıb — yalnız ilk admin hesabı var.
        await db.Users.IgnoreQueryFilters().Where(x => x.TenantId == id).ExecuteDeleteAsync(ct);
        await db.Settings.IgnoreQueryFilters().Where(x => x.TenantId == id).ExecuteDeleteAsync(ct);
        await db.PrivacyPolicies.IgnoreQueryFilters().Where(x => x.TenantId == id).ExecuteDeleteAsync(ct);
        db.Tenants.Remove(tenant);
        await db.SaveChangesAsync(ct);

        await tx.CommitAsync(ct);
    }

    /// <summary>URL üçün qısa ad: hərflər/rəqəmlər saxlanılır, digərləri tireyə çevrilir.</summary>
    private static string NormalizeSlug(string value)
    {
        var sb = new StringBuilder(value.Length);
        var lastDash = false;

        foreach (var ch in value.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(ch))
            {
                sb.Append(ch);
                lastDash = false;
            }
            else if (!lastDash && sb.Length > 0)
            {
                sb.Append('-');
                lastDash = true;
            }
        }

        return sb.ToString().Trim('-');
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
