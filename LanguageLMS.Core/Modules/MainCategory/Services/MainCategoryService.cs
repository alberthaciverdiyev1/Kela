using LanguageLMS.Core.Common.Responses;
using LanguageLMS.Core.Data;
using LanguageLMS.Core.Modules.MainCategory.Requests;
using LanguageLMS.Core.Modules.MainCategory.Responses;
using Microsoft.EntityFrameworkCore;

namespace LanguageLMS.Core.Modules.MainCategory.Services;

using MainCategory = LanguageLMS.Core.Modules.MainCategory.Entities.MainCategory;

public sealed class MainCategoryService(AppDbContext db) : IMainCategoryService
{
    public async Task<PagedResponse<MainCategoryResponse>> ListAsync(MainCategoryListRequest request, CancellationToken ct = default)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var perPage = request.PerPage is < 1 or > 200 ? 20 : request.PerPage;

        var query = db.MainCategories.AsNoTracking();

        if (request.IsDeleted == 1)
        {
            query = query.IgnoreQueryFilters().Where(x => x.DeletedAt != null);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => EF.Functions.ILike(x.Name, $"%{search}%"));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.Id)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync(ct);

        var data = items.Select(ToResponse).ToList();
        var lastPage = (int)Math.Ceiling(total / (double)perPage);

        return new PagedResponse<MainCategoryResponse>(data, page, lastPage, perPage, total);
    }

    public async Task<MainCategoryResponse> GetAsync(long id, CancellationToken ct = default)
    {
        var category = await db.MainCategories
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Main category not found: {id}");

        return ToResponse(category);
    }

    public async Task<MainCategoryResponse> CreateAsync(MainCategoryUpsertRequest request, CancellationToken ct = default)
    {
        await EnsureNameAvailableAsync(request.Name, null, ct);

        var category = new MainCategory
        {
            Name = request.Name,
            UserId = request.UserId
        };

        db.MainCategories.Add(category);
        await db.SaveChangesAsync(ct);

        return ToResponse(category);
    }

    public async Task<MainCategoryResponse> UpdateAsync(long id, MainCategoryUpsertRequest request, CancellationToken ct = default)
    {
        var category = await db.MainCategories
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Main category not found: {id}");

        await EnsureNameAvailableAsync(request.Name, id, ct);

        category.Name = request.Name;
        category.UserId = request.UserId;
        await db.SaveChangesAsync(ct);

        return ToResponse(category);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var category = await db.MainCategories.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Main category not found: {id}");

        category.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    private async Task EnsureNameAvailableAsync(string name, long? excludeId, CancellationToken ct)
    {
        var exists = await db.MainCategories.IgnoreQueryFilters()
            .AnyAsync(x => x.Name == name && (excludeId == null || x.Id != excludeId), ct);

        if (exists)
        {
            throw new ArgumentException("A main category with this name already exists");
        }
    }

    private static MainCategoryResponse ToResponse(MainCategory category) =>
        new(category.Id, category.Name, category.UserId);
}
