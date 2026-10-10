using LanguageLMS.Core.Common.Responses;
using LanguageLMS.Core.Common.Storage;
using LanguageLMS.Core.Common.Bulk;
using LanguageLMS.Core.Common.Sorting;
using LanguageLMS.Core.Data;
using LanguageLMS.Core.Modules.LessonContent.Requests;
using LanguageLMS.Core.Modules.LessonContent.Responses;
using Microsoft.EntityFrameworkCore;

namespace LanguageLMS.Core.Modules.LessonContent.Services;

using LessonContent = LanguageLMS.Core.Modules.LessonContent.Entities.LessonContent;
using LessonContentType = LanguageLMS.Core.Modules.LessonContent.Entities.LessonContentType;
using MediaFile = LanguageLMS.Core.Modules.Media.Entities.MediaFile;
using Video = LanguageLMS.Core.Modules.Media.Entities.Video;
using VoiceRecord = LanguageLMS.Core.Modules.Media.Entities.VoiceRecord;

public sealed class LessonContentService(AppDbContext db, IFileStorage storage) : Abstractions.ILessonContentService
{
    private static readonly TimeSpan UploadUrlExpiry = TimeSpan.FromMinutes(15);

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
            .Include(x => x.Video).Include(x => x.MediaFile).Include(x => x.VoiceRecord)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .Skip((page - 1) * perPage).Take(perPage)
            .ToListAsync(ct);

        var data = items.Select(ToResponse).ToList();
        var lastPage = (int)Math.Ceiling(total / (double)perPage);

        return new PagedResponse<LessonContentResponse>(data, page, lastPage, perPage, total);
    }

    public async Task<LessonContentResponse> GetAsync(long id, CancellationToken ct = default)
    {
        var content = await WithMedia().FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Lesson content not found: {id}");

        return ToResponse(content);
    }

    public async Task<UploadUrlResponse> CreateUploadUrlAsync(
        LessonContentUploadUrlRequest request, CancellationToken ct = default)
    {
        await EnsureLessonExistsAsync(request.LessonId, ct);

        var extension = Path.GetExtension(request.FileName).ToLowerInvariant();
        var folder = FolderFor(request.Type);
        var path = $"{folder}/{folder}-{Guid.NewGuid():N}{extension}";

        var uploadUrl = storage.CreatePresignedPutUrl(path, request.ContentType, UploadUrlExpiry)
            ?? throw new ArgumentException("Presigned upload is not supported by the current storage provider");

        return new UploadUrlResponse(uploadUrl, path);
    }

    public async Task<LessonContentResponse> CreateAsync(LessonContentCreateRequest request, CancellationToken ct = default)
    {
        await EnsureLessonExistsAsync(request.LessonId, ct);

        var path = await ResolvePathAsync(request, ct);

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
                content.MediaFile = new MediaFile { Name = request.File?.FileName, Path = path, SizeBytes = request.File?.Length };
                break;
        }

        db.LessonContents.Add(content);
        await db.SaveChangesAsync(ct);

        return ToResponse(content);
    }

    public async Task<LessonContentResponse> UpdateAsync(
        long id, LessonContentUpdateRequest request, CancellationToken ct = default)
    {
        var content = await db.LessonContents.IgnoreQueryFilters()
            .Include(x => x.Video).Include(x => x.MediaFile).Include(x => x.VoiceRecord)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
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

    private async Task<string> ResolvePathAsync(LessonContentCreateRequest request, CancellationToken ct)
    {
        // birbaşa R2-yə yüklənibsə path verilir; əks halda API-dən keçərək yükləyirik
        if (request.File is not null)
        {
            return await storage.UploadAsync(request.File, FolderFor(request.Type), ct);
        }

        if (!string.IsNullOrWhiteSpace(request.Path))
        {
            return request.Path;
        }

        throw new ArgumentException("File or Path is required");
    }

    private static string FolderFor(LessonContentType type) => type switch
    {
        LessonContentType.Video => "video",
        LessonContentType.Voice => "voice",
        _ => "file"
    };

    private IQueryable<LessonContent> WithMedia() =>
        db.LessonContents.AsNoTracking()
            .Include(x => x.Video).Include(x => x.MediaFile).Include(x => x.VoiceRecord);

    private async Task EnsureLessonExistsAsync(long lessonId, CancellationToken ct)
    {
        if (!await db.Lessons.AnyAsync(x => x.Id == lessonId, ct))
        {
            throw new ArgumentException($"Lesson not found: {lessonId}");
        }
    }

    private LessonContentResponse ToResponse(LessonContent content)
    {
        if (content.Video is not null)
        {
            return Row(content, "video", storage.GetUrl(content.Video.Path), content.Video.PlaybackUrl, content.Video.DurationSeconds);
        }

        if (content.MediaFile is not null)
        {
            return Row(content, "file", storage.GetUrl(content.MediaFile.Path), null, null);
        }

        if (content.VoiceRecord is not null)
        {
            return Row(content, "voice", storage.GetUrl(content.VoiceRecord.Path), null, content.VoiceRecord.DurationSeconds);
        }

        return Row(content, null, null, null, null);
    }

    private static LessonContentResponse Row(LessonContent c, string? type, string? url, string? playbackUrl, int? duration) =>
        new(c.Id, c.LessonId, c.Title, c.Description, type, url, playbackUrl, c.SortOrder, c.IsActive, duration);

    public Task BulkAsync(BulkRequest request, CancellationToken ct = default)
        => BulkHelper.ApplyAsync<LessonContent>(db, request, ct);

    public Task ReorderAsync(SortRequest request, CancellationToken ct = default)
        => SortHelper.ReorderAsync<LessonContent>(db, request, ct);
}
