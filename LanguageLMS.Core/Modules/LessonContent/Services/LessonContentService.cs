using LanguageLMS.Core.Common.Responses;
using LanguageLMS.Core.Common.Storage;
using LanguageLMS.Core.Data;
using LanguageLMS.Core.Modules.LessonContent.Entities;
using LanguageLMS.Core.Modules.LessonContent.Requests;
using LanguageLMS.Core.Modules.LessonContent.Responses;
using Microsoft.EntityFrameworkCore;

namespace LanguageLMS.Core.Modules.LessonContent.Services;

using LessonContent = LanguageLMS.Core.Modules.LessonContent.Entities.LessonContent;
using MediaFile = LanguageLMS.Core.Modules.Media.Entities.MediaFile;
using Video = LanguageLMS.Core.Modules.Media.Entities.Video;
using VoiceRecord = LanguageLMS.Core.Modules.Media.Entities.VoiceRecord;

public sealed class LessonContentService(AppDbContext db, IFileStorage storage) : ILessonContentService
{
    public async Task<PagedResponse<LessonContentResponse>> ListAsync(
        LessonContentListRequest request, CancellationToken ct = default)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var perPage = request.PerPage is < 1 or > 200 ? 50 : request.PerPage;

        var query = db.LessonContents.AsNoTracking();

        if (request.IsDeleted == 1)
        {
            query = query.IgnoreQueryFilters().Where(x => x.DeletedAt != null);
        }

        if (request.LessonId.HasValue)
        {
            query = query.Where(x => x.LessonId == request.LessonId);
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .Include(x => x.Video)
            .Include(x => x.MediaFile)
            .Include(x => x.VoiceRecord)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync(ct);

        var data = items.Select(ToResponse).ToList();
        var lastPage = (int)Math.Ceiling(total / (double)perPage);

        return new PagedResponse<LessonContentResponse>(data, page, lastPage, perPage, total);
    }

    public async Task<LessonContentResponse> GetAsync(long id, CancellationToken ct = default)
    {
        var content = await Query().FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Lesson content not found: {id}");

        return ToResponse(content);
    }

    public async Task<LessonContentResponse> CreateAsync(LessonContentCreateRequest request, CancellationToken ct = default)
    {
        if (!await db.Lessons.AnyAsync(x => x.Id == request.LessonId, ct))
        {
            throw new ArgumentException($"Lesson not found: {request.LessonId}");
        }

        if (request.File is null)
        {
            throw new ArgumentException("File is required");
        }

        var folder = request.Type switch
        {
            LessonContentType.Video => "video",
            LessonContentType.Voice => "voice",
            _ => "file"
        };

        var path = await storage.UploadAsync(request.File, folder, ct);

        var content = new LessonContent
        {
            LessonId = request.LessonId,
            Title = request.Title,
            Description = request.Description,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive
        };

        switch (request.Type)
        {
            case LessonContentType.Video:
                content.Video = new Video { Title = request.Title, Path = path };
                break;
            case LessonContentType.Voice:
                content.VoiceRecord = new VoiceRecord { Title = request.Title, Path = path };
                break;
            default:
                content.MediaFile = new MediaFile { Name = request.File.FileName, Path = path, SizeBytes = request.File.Length };
                break;
        }

        db.LessonContents.Add(content);
        await db.SaveChangesAsync(ct);

        return ToResponse(content);
    }

    public async Task<LessonContentResponse> UpdateAsync(
        long id, LessonContentUpdateRequest request, CancellationToken ct = default)
    {
        var content = await Query(withFilters: false).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Lesson content not found: {id}");

        content.Title = request.Title;
        content.Description = request.Description;
        content.SortOrder = request.SortOrder;
        content.IsActive = request.IsActive;
        await db.SaveChangesAsync(ct);

        return ToResponse(content);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var content = await db.LessonContents.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Lesson content not found: {id}");

        content.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    private IQueryable<LessonContent> Query(bool withFilters = true)
    {
        var query = db.LessonContents.AsNoTracking();
        if (!withFilters) query = db.LessonContents.IgnoreQueryFilters().AsNoTracking();

        return query
            .Include(x => x.Video)
            .Include(x => x.MediaFile)
            .Include(x => x.VoiceRecord);
    }

    private LessonContentResponse ToResponse(LessonContent content)
    {
        if (content.Video is not null)
        {
            return Row(content, "video", storage.GetUrl(content.Video.Path), content.Video.DurationSeconds);
        }

        if (content.MediaFile is not null)
        {
            return Row(content, "file", storage.GetUrl(content.MediaFile.Path), null);
        }

        if (content.VoiceRecord is not null)
        {
            return Row(content, "voice", storage.GetUrl(content.VoiceRecord.Path), content.VoiceRecord.DurationSeconds);
        }

        return Row(content, null, null, null);
    }

    private static LessonContentResponse Row(LessonContent c, string? type, string? url, int? duration) =>
        new(c.Id, c.LessonId, c.Title, c.Description, type, url, c.SortOrder, c.IsActive, duration);
}
