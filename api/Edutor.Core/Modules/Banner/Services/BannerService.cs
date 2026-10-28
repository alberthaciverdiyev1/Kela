using Edutor.Core.Common.Exceptions;
using Edutor.Core.Common.Sorting;
using Edutor.Core.Common.Pagination;
using Edutor.Core.Common.Responses;
using Edutor.Core.Common.Storage;
using Edutor.Core.Common.Bulk;
using Edutor.Core.Data;
using Edutor.Core.Modules.Banner.Entities;
using Edutor.Core.Modules.Banner.Requests;
using Edutor.Core.Modules.Banner.Responses;
using Microsoft.EntityFrameworkCore;

namespace Edutor.Core.Modules.Banner.Services;

using Banner = Edutor.Core.Modules.Banner.Entities.Banner;
using Edutor.Core.Common.Queries;
using Edutor.Core.Common.Authorization;

public sealed class BannerService(AppDbContext db, IFileStorage storage, ITenantFolder folders) : IBannerService
{
    private async Task<string> FolderAsync(CancellationToken ct)
        => $"{await folders.ResolveAsync(ct: ct)}/banners";
    private static readonly TimeSpan NewThreshold = TimeSpan.FromDays(3);

    public async Task<PagedResponse<BannerResponse>> ListAsync(BannerListRequest request, CancellationToken ct = default)
    {
        var (page, perPage) = Paging.Normalize(request);

        var query = db.Banners.AsNoTracking();

        if (request.OnlyActive)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            query = query.Where(x => x.IsActive && (x.ActiveUntil == null || x.ActiveUntil >= today));
        }

        query = query.ApplyTrashFilter(request.IsDeleted);

        query = query.SearchBy(request.Search, x => x.Title);

        var total = await query.CountAsync(ct);
        var items = await query
            .ApplyListSort(request, x => x.Title)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync(ct);

        var now = DateTimeOffset.UtcNow;
        var ownerNames = await OwnerNameMapAsync(items.Select(x => x.UserId), ct);
        var data = items.Select(x => ToResponse(x, now, ownerNames.GetValueOrDefault(x.UserId ?? 0))).ToList();
        return new PagedResponse<BannerResponse>(data, page, Paging.LastPage(total, perPage), perPage, total);
    }

    public async Task<PagedResponse<BannerResponse>> ListForStudentAsync(
        long studentId, BannerListRequest request, CancellationToken ct = default)
    {
        var (page, perPage) = Paging.Normalize(request);

        var query = VisibleForStudent(studentId);

        query = query.SearchBy(request.Search, x => x.Title);

        var total = await query.CountAsync(ct);
        var items = await query
            .ApplyListSort(request, x => x.Title)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync(ct);

        var now = DateTimeOffset.UtcNow;
        var ownerNames = await OwnerNameMapAsync(items.Select(x => x.UserId), ct);
        var data = items.Select(x => ToResponse(x, now, ownerNames.GetValueOrDefault(x.UserId ?? 0))).ToList();
        return new PagedResponse<BannerResponse>(data, page, Paging.LastPage(total, perPage), perPage, total);
    }

    public async Task<BannerResponse> GetForStudentAsync(long studentId, long id, CancellationToken ct = default)
    {
        var banner = await VisibleForStudent(studentId).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Banner not found: {id}");

        return ToResponse(banner, DateTimeOffset.UtcNow, await OwnerNameAsync(banner.UserId, ct));
    }

    /// <summary>
    /// Şagirdin görə biləcəyi story-lər. Global (superadmin) story-lər tenant filtrindən asılı deyil —
    /// filtr yalnız bu sorğuda söndürülür və tenant yoxlaması əl ilə aparılır.
    /// </summary>
    private IQueryable<Banner> VisibleForStudent(long studentId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var tenantId = db.CurrentTenantId;

        return db.Banners
            .IgnoreQueryFilters(new[] { "Tenant" })
            .AsNoTracking()
            .Where(x => x.IsActive && (x.ActiveUntil == null || x.ActiveUntil >= today))
            .Where(x => x.Scope == BannerScope.Global
                || ((tenantId == null || x.TenantId == tenantId)
                    && (x.Scope == BannerScope.Tenant
                        || (x.Scope == BannerScope.Teacher
                            && x.UserId != null
                            && db.Groups.Any(g => g.UserId == x.UserId
                                && db.StudentGroups.Any(sg => sg.GroupId == g.Id && sg.StudentId == studentId))))));
    }

    public async Task<BannerResponse> GetAsync(long id, CancellationToken ct = default)
    {
        var banner = await db.Banners
            .IgnoreQueryFilters(new[] { "SoftDelete" })
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Banner not found: {id}");

        return ToResponse(banner, DateTimeOffset.UtcNow, await OwnerNameAsync(banner.UserId, ct));
    }

    public async Task<BannerResponse> CreateAsync(BannerUpsertRequest request, CancellationToken ct = default)
    {
        var image = request.Image is not null
            ? await storage.UploadAsync(request.Image, await FolderAsync(ct), ct)
            : null;

        var banner = new Banner
        {
            UserId = request.UserId,
            Title = request.Title,
            Image = image,
            IsActive = request.IsActive,
            ActiveUntil = request.ActiveUntil,
            Scope = request.Scope ?? BannerScope.Tenant
        };

        db.Banners.Add(banner);
        await db.SaveChangesAsync(ct);

        return ToResponse(banner, DateTimeOffset.UtcNow, await OwnerNameAsync(banner.UserId, ct));
    }

    public async Task<BannerResponse> UpdateAsync(long id, BannerUpsertRequest request, long? ownerId = null, CancellationToken ct = default)
    {
        var banner = await db.Banners
            .IgnoreQueryFilters(new[] { "SoftDelete" })
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Banner not found: {id}");

        OwnershipGuard.EnsureOwned(ownerId, banner.UserId, "You can only modify your own banner");

        if (request.RemoveImage)
        {
            await storage.DeleteAsync(banner.Image, ct);
            banner.Image = null;
        }
        else if (request.Image is not null)
        {
            await storage.DeleteAsync(banner.Image, ct);
            banner.Image = await storage.UploadAsync(request.Image, await FolderAsync(ct), ct);
        }

        banner.Title = request.Title;
        banner.IsActive = request.IsActive;
        banner.ActiveUntil = request.ActiveUntil;
        await db.SaveChangesAsync(ct);

        return ToResponse(banner, DateTimeOffset.UtcNow, await OwnerNameAsync(banner.UserId, ct));
    }

    public async Task DeleteAsync(long id, long? ownerId = null, CancellationToken ct = default)
    {
        var banner = await db.Banners.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Banner not found: {id}");

        OwnershipGuard.EnsureOwned(ownerId, banner.UserId, "You can only modify your own banner");

        banner.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    private BannerResponse ToResponse(Banner banner, DateTimeOffset now, string? ownerName = null) =>
        new(
            banner.Id,
            banner.Title,
            storage.GetUrl(banner.Image),
            banner.CreatedAt > now.Subtract(NewThreshold),
            banner.IsActive,
            banner.ActiveUntil,
            banner.UserId,
            ownerName);

    private async Task<Dictionary<long, string>> OwnerNameMapAsync(IEnumerable<long?> ids, CancellationToken ct)
    {
        var wanted = ids.Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
        if (wanted.Count == 0) return new Dictionary<long, string>();

        var users = await db.Users.IgnoreQueryFilters().AsNoTracking()
            .Where(x => wanted.Contains(x.Id))
            .Select(x => new { x.Id, x.Name, x.Surname })
            .ToListAsync(ct);

        return users.ToDictionary(x => x.Id, x => $"{x.Name} {x.Surname}".Trim());
    }

    private async Task<string?> OwnerNameAsync(long? ownerId, CancellationToken ct)
        => ownerId is long id
            ? (await OwnerNameMapAsync(new long?[] { id }, ct)).GetValueOrDefault(id)
            : null;

    public Task SetActiveAsync(long id, bool isActive, long? ownerId = null, CancellationToken ct = default)
        => db.SetActiveAsync<Banner>(id, isActive, ownerId, x => x.UserId, "You can only modify your own banner", ct);

    public Task BulkAsync(BulkRequest request, long? ownerId = null, CancellationToken ct = default)
        => db.BulkOwnedAsync<Banner>(request, ownerId, x => x.UserId, "You can only modify your own banners", ct);
}
