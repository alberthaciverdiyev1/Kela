using Kela.Core.Common.Responses;
using Kela.Core.Common.Storage;
using Kela.Core.Common.Bulk;
using Kela.Core.Data;
using Kela.Core.Modules.Banner.Requests;
using Kela.Core.Modules.Banner.Responses;
using Microsoft.EntityFrameworkCore;

namespace Kela.Core.Modules.Banner.Services;

using Banner = Kela.Core.Modules.Banner.Entities.Banner;

public sealed class BannerService(AppDbContext db, IFileStorage storage) : IBannerService
{
    private const string ImageFolder = "banner";
    private static readonly TimeSpan NewThreshold = TimeSpan.FromDays(3);

    public async Task<PagedResponse<BannerResponse>> ListAsync(BannerListRequest request, CancellationToken ct = default)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var perPage = request.PerPage < 1 ? 20 : Math.Min(request.PerPage, 500);

        var query = db.Banners.AsNoTracking();

        if (request.IsDeleted == 1)
        {
            query = query.IgnoreQueryFilters().Where(x => x.DeletedAt != null);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => EF.Functions.ILike(x.Title, $"%{search}%"));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.Id)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync(ct);

        var now = DateTimeOffset.UtcNow;
        var data = items.Select(x => ToResponse(x, now)).ToList();
        var lastPage = (int)Math.Ceiling(total / (double)perPage);

        return new PagedResponse<BannerResponse>(data, page, lastPage, perPage, total);
    }

    public async Task<BannerResponse> GetAsync(long id, CancellationToken ct = default)
    {
        var banner = await db.Banners
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Banner not found: {id}");

        return ToResponse(banner, DateTimeOffset.UtcNow);
    }

    public async Task<BannerResponse> CreateAsync(BannerUpsertRequest request, CancellationToken ct = default)
    {
        var image = request.Image is not null
            ? await storage.UploadAsync(request.Image, ImageFolder, ct)
            : null;

        var banner = new Banner
        {
            Title = request.Title,
            Image = image
        };

        db.Banners.Add(banner);
        await db.SaveChangesAsync(ct);

        return ToResponse(banner, DateTimeOffset.UtcNow);
    }

    public async Task<BannerResponse> UpdateAsync(long id, BannerUpsertRequest request, CancellationToken ct = default)
    {
        var banner = await db.Banners
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Banner not found: {id}");

        if (request.RemoveImage)
        {
            await storage.DeleteAsync(banner.Image, ct);
            banner.Image = null;
        }
        else if (request.Image is not null)
        {
            await storage.DeleteAsync(banner.Image, ct);
            banner.Image = await storage.UploadAsync(request.Image, ImageFolder, ct);
        }

        banner.Title = request.Title;
        await db.SaveChangesAsync(ct);

        return ToResponse(banner, DateTimeOffset.UtcNow);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var banner = await db.Banners.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Banner not found: {id}");

        banner.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    private BannerResponse ToResponse(Banner banner, DateTimeOffset now) =>
        new(
            banner.Id,
            banner.Title,
            storage.GetUrl(banner.Image),
            banner.CreatedAt > now.Subtract(NewThreshold));

    public Task BulkAsync(BulkRequest request, CancellationToken ct = default)
        => BulkHelper.ApplyAsync<Banner>(db, request, ct);
}
