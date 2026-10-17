using System.Text.Json;
using Kela.Core.Common.Responses;
using Kela.Core.Common.Storage;
using Kela.Core.Common.Bulk;
using Kela.Core.Common.Sorting;
using Kela.Core.Data;
using Kela.Core.Modules.Lesson.Entities;
using Kela.Core.Modules.Lesson.Requests;
using Kela.Core.Modules.Lesson.Responses;
using Microsoft.EntityFrameworkCore;

namespace Kela.Core.Modules.Lesson.Services;

using Lesson = Kela.Core.Modules.Lesson.Entities.Lesson;

public sealed class LessonService(AppDbContext db, IFileStorage storage, ITenantFolder folders) : ILessonService
{
    private async Task<string> FolderAsync(CancellationToken ct)
        => $"{await folders.ResolveAsync(ct: ct)}/lessons";

    public async Task<PagedResponse<LessonResponse>> ListAsync(LessonListRequest request, CancellationToken ct = default)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var perPage = request.PerPage < 1 ? 20 : Math.Min(request.PerPage, 500);

        var query = db.Lessons.AsNoTracking();

        if (request.IsDeleted == 1)
        {
            query = query.IgnoreQueryFilters(new[] { "SoftDelete" }).Where(x => x.DeletedAt != null);
        }

        if (request.SubjectId.HasValue)
        {
            query = query.Where(x => x.SubjectId == request.SubjectId);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => EF.Functions.ILike(x.Title, $"%{search}%"));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .ApplyListSort(request, x => x.Title, x => x.SortOrder)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync(ct);

        var data = items.Select(ToResponse).ToList();
        var lastPage = (int)Math.Ceiling(total / (double)perPage);

        return new PagedResponse<LessonResponse>(data, page, lastPage, perPage, total);
    }

    public async Task<LessonResponse> GetAsync(long id, CancellationToken ct = default)
    {
        var lesson = await db.Lessons.IgnoreQueryFilters(new[] { "SoftDelete" }).AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Lesson not found: {id}");

        return ToResponse(lesson);
    }

    public async Task<LessonResponse> CreateAsync(LessonUpsertRequest request, CancellationToken ct = default)
    {
        await EnsureSubjectExistsAsync(request.SubjectId, ct);

        var image = request.Image is not null
            ? await storage.UploadAsync(request.Image, await FolderAsync(ct), ct)
            : null;

        var lesson = new Lesson
        {
            SubjectId = request.SubjectId,
            Title = request.Title,
            Image = image,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive,
            Settings = string.IsNullOrWhiteSpace(request.Settings) ? LessonDefaults.Settings : request.Settings
        };

        db.Lessons.Add(lesson);
        await db.SaveChangesAsync(ct);

        return ToResponse(lesson);
    }

    public async Task<LessonResponse> UpdateAsync(long id, LessonUpsertRequest request, CancellationToken ct = default)
    {
        var lesson = await db.Lessons.IgnoreQueryFilters(new[] { "SoftDelete" })
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Lesson not found: {id}");

        await EnsureSubjectExistsAsync(request.SubjectId, ct);

        if (request.RemoveImage)
        {
            await storage.DeleteAsync(lesson.Image, ct);
            lesson.Image = null;
        }
        else if (request.Image is not null)
        {
            await storage.DeleteAsync(lesson.Image, ct);
            lesson.Image = await storage.UploadAsync(request.Image, await FolderAsync(ct), ct);
        }

        lesson.SubjectId = request.SubjectId;
        lesson.Title = request.Title;
        lesson.SortOrder = request.SortOrder;
        lesson.IsActive = request.IsActive;
        if (!string.IsNullOrWhiteSpace(request.Settings)) lesson.Settings = request.Settings;
        await db.SaveChangesAsync(ct);

        return ToResponse(lesson);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var lesson = await db.Lessons.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Lesson not found: {id}");

        lesson.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    private async Task EnsureSubjectExistsAsync(long subjectId, CancellationToken ct)
    {
        if (!await db.Subjects.AnyAsync(x => x.Id == subjectId, ct))
        {
            throw new ArgumentException($"Subject not found: {subjectId}");
        }
    }

    private LessonResponse ToResponse(Lesson lesson) =>
        new(
            lesson.Id,
            lesson.SubjectId,
            lesson.Title,
            storage.GetUrl(lesson.Image),
            lesson.SortOrder,
            lesson.IsActive,
            lesson.Settings);

    public Task BulkAsync(BulkRequest request, CancellationToken ct = default)
        => BulkHelper.ApplyAsync<Lesson>(db, request, ct);

    public Task ReorderAsync(SortRequest request, CancellationToken ct = default)
        => SortHelper.ReorderAsync<Lesson>(db, request, ct);

    public async Task<LessonSettingsResponse> GetSettingsAsync(long id, CancellationToken ct = default)
    {
        var lesson = await db.Lessons.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Lesson not found: {id}");

        return LessonSettings.Parse(lesson.Settings);
    }

    public async Task<LessonSettingsResponse> SetSettingsAsync(long id, SetLessonSettingsRequest request, CancellationToken ct = default)
    {
        var lesson = await db.Lessons.IgnoreQueryFilters(new[] { "SoftDelete" }).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Lesson not found: {id}");

        lesson.Settings = LessonSettings.Serialize(request);
        await db.SaveChangesAsync(ct);
        return LessonSettings.Parse(lesson.Settings);
    }
}
