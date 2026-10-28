using System.Text;
using Edutor.Core.Common.Localization;
using Edutor.Core.Common.MultiTenancy;
using Edutor.Core.Common.Storage;
using Edutor.Core.Data;
using Edutor.Core.Modules.Tenant.Requests;
using Edutor.Core.Modules.Tenant.Responses;
using Edutor.Core.Modules.User.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Edutor.Core.Modules.Tenant.Services;

using Tenant = Edutor.Core.Modules.Tenant.Entities.Tenant;
using Setting = Edutor.Core.Modules.Setting.Entities.Setting;
using User = Edutor.Core.Modules.User.Entities.User;

public sealed class TenantService(
    AppDbContext db,
    ITenantProvider tenantProvider,
    IFileStorage storage,
    ITenantFolder folders,
    IStringLocalizer<SharedResource> L) : ITenantService
{
    public async Task<IReadOnlyList<TenantResponse>> ListAsync(CancellationToken ct = default)
    {
        var tenants = await db.Tenants.AsNoTracking().Where(x => x.DeletedAt == null).OrderBy(x => x.Id)
            .Select(x => new { x.Id, x.Name, x.Slug, x.IsActive, x.IsPending, x.PlanId, x.PlanExpiresAt })
            .ToListAsync(ct);

        var logos = await LogoMapAsync(ct);

        var plans = await db.SubscriptionPlans.AsNoTracking()
            .Select(x => new { x.Id, x.Name })
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        return tenants.Select(t => new TenantResponse(
            t.Id, t.Name, t.Slug, t.IsActive, Lookup(logos, t.Id), t.IsPending,
            t.PlanId,
            t.PlanId is long id && plans.TryGetValue(id, out var name) ? name : null,
            t.PlanExpiresAt)).ToList();
    }

    public async Task<IReadOnlyList<TenantResponse>> ListPublicAsync(CancellationToken ct = default)
    {
        var tenants = await db.Tenants.AsNoTracking().Where(x => x.IsActive && x.DeletedAt == null).OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name, x.Slug, x.IsActive, x.IsPending })
            .ToListAsync(ct);

        var logos = await LogoMapAsync(ct);
        return tenants.Select(t => new TenantResponse(t.Id, t.Name, t.Slug, t.IsActive, Lookup(logos, t.Id), t.IsPending)).ToList();
    }

    public async Task<TenantResponse?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var t = await db.Tenants.AsNoTracking().Where(x => x.Id == id && x.DeletedAt == null)
            .Select(x => new { x.Id, x.Name, x.Slug, x.IsActive, x.IsPending })
            .FirstOrDefaultAsync(ct);

        if (t is null) return null;
        return new TenantResponse(t.Id, t.Name, t.Slug, t.IsActive, await LogoAsync(t.Id, ct), t.IsPending);
    }

    public async Task<TenantResponse?> GetCurrentAsync(CancellationToken ct = default)
    {
        if (tenantProvider.TenantId is not long id) return null;

        var t = await db.Tenants.AsNoTracking().Where(x => x.Id == id && x.DeletedAt == null)
            .Select(x => new { x.Id, x.Name, x.Slug, x.IsActive, x.IsPending })
            .FirstOrDefaultAsync(ct);

        if (t is null) return null;
        return new TenantResponse(t.Id, t.Name, t.Slug, t.IsActive, await LogoAsync(t.Id, ct), t.IsPending);
    }

    public async Task<string> GetCurrentAccountTypeAsync(CancellationToken ct = default)
    {
        if (tenantProvider.TenantId is not long tenantId) return "course";

        var type = await db.Tenants.AsNoTracking()
            .Where(x => x.Id == tenantId)
            .Select(x => x.AccountType)
            .FirstOrDefaultAsync(ct);

        return string.IsNullOrWhiteSpace(type) ? "course" : type;
    }

    public async Task<TenantResponse> UpdateCurrentAsync(UpdateTenantProfileRequest request, CancellationToken ct = default)
    {
        if (tenantProvider.TenantId is not long id)
        {
            throw new InvalidOperationException("Tenant context is required");
        }

        var tenant = await db.Tenants.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, ct)
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

        if (await db.Tenants.AnyAsync(x => x.Slug == slug && x.DeletedAt == null, ct))
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
        var adminName = request.AdminName.Trim();
        if (adminName.Length == 0) throw new ArgumentException(L["Tenant_AdminNameRequired"]);

        // Fərdi müəllim öz adı ilə tək nəfərlik tenant kimi qeydiyyatdan keçir.
        var isTeacher = string.Equals(request.AccountType, "teacher", StringComparison.OrdinalIgnoreCase);
        var surname = request.AdminSurname?.Trim();

        var courseName = isTeacher
            ? $"{adminName} {surname}".Trim()
            : request.CourseName.Trim();

        if (courseName.Length == 0) throw new ArgumentException(L["Tenant_CourseNameRequired"]);
        if (courseName.Length > 190) courseName = courseName[..190];

        var baseSlug = NormalizeSlug(courseName);
        if (baseSlug.Length == 0) baseSlug = isTeacher ? "muellim" : "kurs";
        var slug = await UniqueSlugAsync(baseSlug, ct);

        var phone = request.PhoneNumber.Trim();
        if (phone.Length == 0) throw new ArgumentException(L["Validation_PhoneRequired"]);

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

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        // Təsdiq gözlənilmir — kurs dərhal aktiv yaradılır (subscription sonra əlavə olunacaq).
        var tenant = new Tenant
        {
            Name = courseName,
            Slug = slug,
            IsActive = true,
            IsPending = false,
            AccountType = isTeacher ? "teacher" : "course"
        };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync(ct);

        db.Users.Add(new User
        {
            TenantId = tenant.Id,
            Name = adminName,
            Surname = surname,
            Email = email,
            PhoneNumber = phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            // Fərdi müəllim müəllim kimi daxil olur; kurs sahibi isə admin olur.
            Type = isTeacher ? UserTypes.Teacher : UserTypes.Admin
        });

        if (!isTeacher && request.Logo is not null)
        {
            var folder = $"{await folders.ResolveAsync(tenant.Id, ct)}/settings";
            var logo = await storage.UploadAsync(request.Logo, folder, ct);

            db.Settings.Add(new Setting
            {
                TenantId = tenant.Id,
                Logo = logo,
                Email = email
            });
        }

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return new TenantResponse(tenant.Id, tenant.Name, tenant.Slug, tenant.IsActive, IsPending: false);
    }

    public async Task<TenantResponse> SetActiveAsync(long id, bool isActive, CancellationToken ct = default)
    {
        var tenant = await db.Tenants.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, ct)
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
        var tenant = await db.Tenants.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, ct)
            ?? throw new KeyNotFoundException($"Tenant not found: {id}");

        // Təsdiqlənməmiş müraciətin məlumatı yoxdur — fiziki silinir, slug yenidən istifadə oluna bilsin.
        if (!tenant.IsPending)
        {
            // Real tenant-ın məlumatı qorunur: giriş bağlanır, siyahılardan çıxır.
            tenant.IsActive = false;
            tenant.IsPending = false;
            tenant.DeletedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);
            return;
        }

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        await db.Users.IgnoreQueryFilters().Where(x => x.TenantId == id).ExecuteDeleteAsync(ct);
        await db.Settings.IgnoreQueryFilters().Where(x => x.TenantId == id).ExecuteDeleteAsync(ct);
        await db.PrivacyPolicies.IgnoreQueryFilters().Where(x => x.TenantId == id).ExecuteDeleteAsync(ct);
        db.Tenants.Remove(tenant);
        await db.SaveChangesAsync(ct);

        await tx.CommitAsync(ct);
    }

    /// <summary>Ad əsasında unikal slug yaradır (mövcuddursa "-2", "-3" əlavə edir).</summary>
    private async Task<string> UniqueSlugAsync(string baseSlug, CancellationToken ct)
    {
        var root = baseSlug.Length > 90 ? baseSlug[..90] : baseSlug;
        var slug = root;
        var suffix = 2;

        while (await db.Tenants.AnyAsync(x => x.Slug == slug && x.DeletedAt == null, ct))
        {
            var tail = "-" + suffix++;
            slug = root[..Math.Min(root.Length, 100 - tail.Length)] + tail;
        }

        return slug;
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
