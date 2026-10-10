using LanguageLMS.Core.Common.Responses;
using LanguageLMS.Core.Common.Storage;
using LanguageLMS.Core.Data;
using LanguageLMS.Core.Modules.Profession.Requests;
using LanguageLMS.Core.Modules.Profession.Responses;
using Microsoft.EntityFrameworkCore;

namespace LanguageLMS.Core.Modules.Profession.Services;

using Profession = LanguageLMS.Core.Modules.Profession.Entities.Profession;

public sealed class ProfessionService(AppDbContext db, IFileStorage storage) : IProfessionService
{
    private const string ImageFolder = "profession";

    public async Task<PagedResponse<ProfessionResponse>> ListAsync(ProfessionListRequest request, CancellationToken ct = default)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var perPage = request.PerPage is < 1 or > 200 ? 20 : request.PerPage;

        var query = db.Professions.AsNoTracking();

        if (request.IsDeleted == 1)
        {
            query = query.IgnoreQueryFilters().Where(x => x.DeletedAt != null);
        }

        if (request.ParentId.HasValue)
        {
            query = query.Where(x => x.ParentId == request.ParentId);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => EF.Functions.ILike(x.Name, $"%{search}%"));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .Include(x => x.Children)
            .OrderByDescending(x => x.Id)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync(ct);

        var data = items.Select(ToResponse).ToList();
        var lastPage = (int)Math.Ceiling(total / (double)perPage);

        return new PagedResponse<ProfessionResponse>(data, page, lastPage, perPage, total);
    }

    public async Task<ProfessionResponse> GetAsync(long id, CancellationToken ct = default)
    {
        var profession = await db.Professions
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(x => x.Children)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Profession not found: {id}");

        return ToResponse(profession);
    }

    public async Task<ProfessionResponse> CreateAsync(ProfessionUpsertRequest request, CancellationToken ct = default)
    {
        await EnsureNameAvailableAsync(request.Name, null, ct);
        await EnsureParentValidAsync(request.ParentId, null, ct);

        var image = request.Image is not null
            ? await storage.UploadAsync(request.Image, ImageFolder, ct)
            : null;

        var profession = new Profession
        {
            Name = request.Name,
            Image = image,
            ParentId = request.ParentId,
            UserId = request.UserId
        };

        db.Professions.Add(profession);
        await db.SaveChangesAsync(ct);

        return ToResponse(profession);
    }

    public async Task<ProfessionResponse> UpdateAsync(long id, ProfessionUpsertRequest request, CancellationToken ct = default)
    {
        var profession = await db.Professions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Profession not found: {id}");

        await EnsureNameAvailableAsync(request.Name, id, ct);
        await EnsureParentValidAsync(request.ParentId, id, ct);

        if (request.RemoveImage)
        {
            await storage.DeleteAsync(profession.Image, ct);
            profession.Image = null;
        }
        else if (request.Image is not null)
        {
            await storage.DeleteAsync(profession.Image, ct);
            profession.Image = await storage.UploadAsync(request.Image, ImageFolder, ct);
        }

        profession.Name = request.Name;
        profession.ParentId = request.ParentId;
        profession.UserId = request.UserId;
        await db.SaveChangesAsync(ct);

        return ToResponse(profession);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var profession = await db.Professions.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Profession not found: {id}");

        if (await db.Professions.AnyAsync(x => x.ParentId == id, ct))
        {
            throw new ArgumentException("Cannot delete a profession that has subcategories");
        }

        profession.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    private async Task EnsureNameAvailableAsync(string name, long? excludeId, CancellationToken ct)
    {
        var exists = await db.Professions.IgnoreQueryFilters()
            .AnyAsync(x => x.Name == name && (excludeId == null || x.Id != excludeId), ct);

        if (exists)
        {
            throw new ArgumentException("A profession with this name already exists");
        }
    }

    private async Task EnsureParentValidAsync(long? parentId, long? selfId, CancellationToken ct)
    {
        if (parentId is null) return;

        if (selfId is not null && parentId == selfId)
        {
            throw new ArgumentException("A profession cannot be its own parent");
        }

        var parent = await db.Professions.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == parentId, ct)
            ?? throw new ArgumentException("Parent profession not found");

        if (parent.ParentId is not null)
        {
            throw new ArgumentException("Only a top-level profession can be selected as parent");
        }

        if (selfId is not null && await db.Professions.AnyAsync(x => x.ParentId == selfId, ct))
        {
            throw new ArgumentException("A profession with subcategories cannot be moved under another profession");
        }
    }

    private ProfessionResponse ToResponse(Profession profession) =>
        new(
            profession.Id,
            profession.Name,
            storage.GetUrl(profession.Image),
            profession.ParentId,
            profession.Children
                .Select(c => new ProfessionRef(c.Id, c.Name, storage.GetUrl(c.Image)))
                .ToList());
}
