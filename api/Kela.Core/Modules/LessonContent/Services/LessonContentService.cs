using Kela.Core.Modules.Media.Entities;
using Kela.Core.Common.Responses;
using Kela.Core.Common.Storage;
using Kela.Core.Common.Bulk;
using Kela.Core.Common.Sorting;
using Kela.Core.Data;
using Kela.Core.Modules.LessonContent.Requests;
using Kela.Core.Modules.LessonContent.Responses;
using Microsoft.EntityFrameworkCore;

namespace Kela.Core.Modules.LessonContent.Services;

using LessonContent = Kela.Core.Modules.LessonContent.Entities.LessonContent;
using LessonContentLesson = Kela.Core.Modules.LessonContent.Entities.LessonContentLesson;
using LessonContentType = Kela.Core.Modules.LessonContent.Entities.LessonContentType;
using MediaFile = Kela.Core.Modules.Media.Entities.MediaFile;
using Video = Kela.Core.Modules.Media.Entities.Video;
using VoiceRecord = Kela.Core.Modules.Media.Entities.VoiceRecord;

public sealed class LessonContentService(
    AppDbContext db,
    IFileStorage storage,
    ITenantFolder folders,
    Kela.Core.Modules.Nodes.Abstractions.IVideoJobScheduler jobs)
    : Abstractions.ILessonContentService
{
    private static readonly TimeSpan UploadUrlExpiry = TimeSpan.FromMinutes(30);

    public async Task<PagedResponse<LessonContentResponse>> ListAsync(
        LessonContentListRequest request, CancellationToken ct = default)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var perPage = request.PerPage < 1 ? 50 : Math.Min(request.PerPage, 500);

        var query = db.LessonContents.AsNoTracking();

        if (request.IsDeleted == 1)
        {
            query = query.IgnoreQueryFilters(new[] { "SoftDelete" }).Where(x => x.DeletedAt != null);
        }

        if (request.LessonId.HasValue)
        {
            var lessonId = request.LessonId.Value;
            query = query.Where(x => x.LessonId == lessonId
                                     || x.AdditionalLessons.Any(l => l.LessonId == lessonId));
        }

        if (request.UserId is long ownerId)
        {
            // Teacher yalnız öz yaratdığı materialları görür; admin materialları hamıya açıqdır.
            query = query.Where(x => x.UserId == ownerId || x.UserId == null);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = $"%{request.Search.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.Title, term));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .Include(x => x.Video).Include(x => x.MediaFile).Include(x => x.VoiceRecord)
            .Include(x => x.AdditionalLessons)
            .ApplyListSort(request, x => x.Title, x => x.SortOrder)
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

        var folder = await FolderForAsync(request.Type, await LessonTenantAsync(request.LessonId, ct), ct);
        var path = await storage.BuildUniquePathAsync(folder, request.FileName, ct);

        var uploadUrl = storage.CreatePresignedPutUrl(path, request.ContentType, UploadUrlExpiry)
            ?? throw new ArgumentException("Presigned upload is not supported by the current storage provider");

        return new UploadUrlResponse(uploadUrl, path);
    }

    private static readonly TimeSpan MultipartUrlExpiry = TimeSpan.FromHours(6);

    public async Task<MultipartTicket?> StartMultipartAsync(LessonContentUploadUrlRequest request, CancellationToken ct = default)
    {
        await EnsureLessonExistsAsync(request.LessonId, ct);

        var folder = await FolderForAsync(request.Type, await LessonTenantAsync(request.LessonId, ct), ct);
        return await storage.StartMultipartAsync(folder, request.FileName, request.ContentType, ct);
    }

    public IReadOnlyList<MultipartPartUrl> SignParts(MultipartPartsRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Path) || string.IsNullOrWhiteSpace(request.UploadId))
            throw new ArgumentException("path and uploadId are required");

        if (request.PartNumbers.Count == 0)
            throw new ArgumentException("partNumbers is empty");

        return storage.SignParts(request.Path, request.UploadId, request.PartNumbers, MultipartUrlExpiry);
    }

    public Task CompleteMultipartAsync(MultipartCompleteRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Path) || string.IsNullOrWhiteSpace(request.UploadId))
            throw new ArgumentException("path and uploadId are required");

        if (request.Parts.Count == 0)
            throw new ArgumentException("parts is empty");

        return storage.CompleteMultipartAsync(request.Path, request.UploadId, request.Parts, ct);
    }

    public Task AbortMultipartAsync(MultipartAbortRequest request, CancellationToken ct = default)
        => storage.AbortMultipartAsync(request.Path, request.UploadId, ct);

    public async Task<LessonContentResponse> CreateAsync(LessonContentCreateRequest request, CancellationToken ct = default)
    {
        // Tenant dərsdən götürülür. Superadmin tenant seçmədən yükləyə bilər — o halda
        // sorğu kontekstində tenant olmur və material səhvən tenant 0-a düşərdi; nəticədə
        // həmin videonun konversiya tapşırıqlarını heç bir cihaz görməzdi.
        var tenantId = await LessonTenantAsync(request.LessonId, ct)
            ?? throw new ArgumentException($"Lesson not found: {request.LessonId}");

        var path = await ResolvePathAsync(request, ct);

        var additionalLessons = await ResolveAdditionalLessonsAsync(request.LessonIds, request.LessonId, tenantId, ct);

        var content = new LessonContent
        {
            LessonId = request.LessonId,
            UserId = request.UserId,
            TenantId = tenantId,
            Title = request.Title,
            Description = request.Description,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive
        };

        foreach (var lessonId in additionalLessons)
        {
            content.AdditionalLessons.Add(new LessonContentLesson { LessonId = lessonId, TenantId = tenantId });
        }

        if (path is not null)
        {
            switch (request.Type)
            {
                case LessonContentType.Video:
                    content.Video = new Video { TenantId = tenantId, Title = request.Title, Path = path };
                    break;
                case LessonContentType.Voice:
                    content.VoiceRecord = new VoiceRecord { TenantId = tenantId, Title = request.Title, Path = path };
                    break;
                default:
                    content.MediaFile = new MediaFile
                    {
                        TenantId = tenantId,
                        Name = request.File?.FileName,
                        Path = path,
                        SizeBytes = request.File?.Length
                    };
                    break;
            }
        }

        db.LessonContents.Add(content);
        await db.SaveChangesAsync(ct);

        // Video yüklənən kimi konversiya tapşırıqları yaranır — worker onları götürə bilər.
        if (content.Video is not null) await jobs.EnsureJobsAsync(content.Video.Id, ct);

        return ToResponse(content);
    }

    /// <summary>
    /// Fon rejimində yüklənmiş faylı mövcud materiala bağlayır. Böyük videoda istifadəçi
    /// yükləməni gözləmir: material dərhal yaranır, fayl bitəndə bura gəlir.
    /// </summary>
    public async Task<LessonContentResponse> AttachFileAsync(
        long id, LessonContentAttachRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Path))
            throw new ArgumentException("path is required");

        var content = await db.LessonContents
            .Include(x => x.Video).Include(x => x.MediaFile).Include(x => x.VoiceRecord)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Lesson content not found: {id}");

        switch (request.Type)
        {
            case LessonContentType.Video:
                content.Video ??= new Video { TenantId = content.TenantId, Title = content.Title };
                content.Video.Path = request.Path;
                content.Video.ConversionStatus = VideoStatus.Pending;
                break;

            case LessonContentType.Voice:
                content.VoiceRecord ??= new VoiceRecord { TenantId = content.TenantId, Title = content.Title };
                content.VoiceRecord.Path = request.Path;
                break;

            default:
                content.MediaFile ??= new MediaFile { TenantId = content.TenantId, Name = content.Title };
                content.MediaFile.Path = request.Path;
                break;
        }

        await db.SaveChangesAsync(ct);

        // Video bağlanan kimi konversiya tapşırıqları yaranmalıdır — worker onları gözləyir.
        if (content.Video is not null) await jobs.EnsureJobsAsync(content.Video.Id, ct);

        return await GetAsync(id, ct);
    }

    public async Task<LessonContentResponse> UpdateAsync(
        long id, LessonContentUpdateRequest request, CancellationToken ct = default)
    {
        var content = await db.LessonContents.IgnoreQueryFilters(new[] { "SoftDelete" })
            .Include(x => x.Video).Include(x => x.MediaFile).Include(x => x.VoiceRecord)
            .Include(x => x.AdditionalLessons)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Lesson content not found: {id}");

        var additionalLessons = await ResolveAdditionalLessonsAsync(request.LessonIds, content.LessonId, content.TenantId, ct);
        var previousLinks = content.AdditionalLessons.ToList();
        content.AdditionalLessons.Clear();
        db.LessonContentLessons.RemoveRange(previousLinks);

        foreach (var lessonId in additionalLessons)
        {
            content.AdditionalLessons.Add(new LessonContentLesson { LessonId = lessonId, TenantId = content.TenantId });
        }

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

    private async Task<string?> ResolvePathAsync(LessonContentCreateRequest request, CancellationToken ct)
    {
        // birbaşa R2-yə yüklənibsə path verilir; əks halda API-dən keçərək yükləyirik
        if (request.File is not null)
        {
            var folder = await FolderForAsync(request.Type, await LessonTenantAsync(request.LessonId, ct), ct);
            return await storage.UploadAsync(request.File, folder, ct);
        }

        if (!string.IsNullOrWhiteSpace(request.Path))
        {
            // İstemci birbaşa R2-yə yükləyir; yolun həqiqətən orada olduğunu yoxlamasaq,
            // yükləmə yarımçıq qalsa DB-də mövcud olmayan fayl qeyd olunardı.
            if (!await storage.ExistsAsync(request.Path, ct))
                throw new ArgumentException("Uploaded file was not found in storage");

            return request.Path;
        }

        // Nə fayl, nə yol verilməyibsə material mediasız yaranır: böyük video fonda
        // yüklənir və bitəndə AttachFileAsync ilə bağlanır.
        return null;
    }

    /// <summary>
    /// Bucket quruluşu tenant üzrə ayrılır:
    /// <c>{tenant}/videos/raw</c>, <c>{tenant}/videos/m3u8</c>, <c>{tenant}/voices</c>, <c>{tenant}/files</c>.
    /// Konversiyadan sonra worker m3u8-i <c>{tenant}/videos/m3u8</c> altına yazır və həmin
    /// açarı callback ilə bildirir.
    /// </summary>
    private async Task<string> FolderForAsync(LessonContentType type, long? fallbackTenantId, CancellationToken ct)
    {
        var prefix = await folders.ResolveAsync(fallbackTenantId, ct);

        return type switch
        {
            LessonContentType.Video => $"{prefix}/videos/raw",
            LessonContentType.Voice => $"{prefix}/voices",
            _ => $"{prefix}/files"
        };
    }


    private IQueryable<LessonContent> WithMedia() =>
        db.LessonContents.AsNoTracking()
            .Include(x => x.Video).Include(x => x.MediaFile).Include(x => x.VoiceRecord)
            .Include(x => x.AdditionalLessons);

    /// <summary>Dərsin tenant-ı — materialın tenant-ı da ondan götürülür.</summary>
    private Task<long?> LessonTenantAsync(long lessonId, CancellationToken ct)
        => db.Lessons.AsNoTracking().Where(x => x.Id == lessonId)
            .Select(x => (long?)x.TenantId).FirstOrDefaultAsync(ct);

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
            return Row(content, "video", storage.GetUrl(content.Video.Path), ResolvePlayback(content.Video.PlaybackUrl),
            content.Video.DurationSeconds,
            string.IsNullOrWhiteSpace(content.Video.Thumbnail) ? null : storage.GetUrl(content.Video.Thumbnail),
            content.Video.ConversionStatus);
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

    /// <summary>
    /// Worker m3u8 açarını (nisbi yol) bildirə bilər — o halda URL-ə çeviririk.
    /// Artıq tam URL-dirsə storage onu olduğu kimi qaytarır.
    /// </summary>
    private string? ResolvePlayback(string? playbackUrl)
        => string.IsNullOrWhiteSpace(playbackUrl) ? null : storage.GetUrl(playbackUrl);

    private static LessonContentResponse Row(LessonContent c, string? type, string? url, string? playbackUrl,
        int? duration, string? thumbnailUrl = null, string? conversionStatus = null) =>
        new(c.Id, c.LessonId, c.Title, c.Description, type, url, playbackUrl, c.SortOrder, c.IsActive,
            duration, thumbnailUrl, conversionStatus,
            c.AdditionalLessons.Select(l => l.LessonId).ToList());

    /// <summary>Əlavə dərsləri yoxlayır: mövcud olmalı və materialın tenant-ına aid olmalıdır.</summary>
    private async Task<List<long>> ResolveAdditionalLessonsAsync(
        IEnumerable<long>? lessonIds, long primaryLessonId, long? tenantId, CancellationToken ct)
    {
        var wanted = (lessonIds ?? Enumerable.Empty<long>())
            .Where(id => id != primaryLessonId)
            .Distinct()
            .ToList();

        if (wanted.Count == 0) return wanted;

        var found = await db.Lessons.AsNoTracking()
            .Where(x => wanted.Contains(x.Id))
            .Select(x => new { x.Id, x.TenantId })
            .ToListAsync(ct);

        if (found.Count != wanted.Count)
        {
            throw new ArgumentException("Some of the selected lessons were not found");
        }

        if (tenantId.HasValue && found.Any(x => x.TenantId != tenantId))
        {
            throw new ArgumentException("Lesson content can only be shared with lessons from the same tenant");
        }

        return found.Select(x => x.Id).ToList();
    }

    public Task BulkAsync(BulkRequest request, CancellationToken ct = default)
        => BulkHelper.ApplyAsync<LessonContent>(db, request, ct);

    public Task ReorderAsync(SortRequest request, CancellationToken ct = default)
        => SortHelper.ReorderAsync<LessonContent>(db, request, ct);
}
