using System.Text.Json;
using Kela.Core.Modules.Media.Entities;
using Kela.Core.Common.Pagination;
using Kela.Core.Common.Responses;
using Kela.Core.Common.Exceptions;
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
        var (page, perPage) = Paging.Normalize(request, defaultPerPage: 50);

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
            // Teacher öz materialını və paylaşılan fənnin dərslərindəki materialları görür.
            query = query.Where(x => x.UserId == ownerId
                || db.Lessons.Any(l => l.Id == x.LessonId && l.Subject!.IsShared)
                || x.AdditionalLessons.Any(al => db.Lessons.Any(l => l.Id == al.LessonId && l.Subject!.IsShared)));
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = $"%{request.Search.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.Title, term));
        }

        var total = await query.CountAsync(ct);

        List<LessonContent> items;

        // Dərsə görə filtrləndikdə "əl ilə sıra" paylaşılan materiallar üçün link sırasını istifadə edir.
        var useLessonOrder = request.LessonId is long lid
                             && string.Equals(request.SortBy, ListSort.Order, StringComparison.OrdinalIgnoreCase);

        if (useLessonOrder)
        {
            var all = await query
                .Include(x => x.Video).Include(x => x.MediaFile).Include(x => x.VoiceRecord)
                .Include(x => x.AdditionalLessons)
                .ToListAsync(ct);

            var lessonId = request.LessonId!.Value;
            var ordered = ListSort.IsAscending(request)
                ? all.OrderBy(x => EffectiveSort(x, lessonId)).ThenBy(x => x.Id)
                : all.OrderByDescending(x => EffectiveSort(x, lessonId)).ThenBy(x => x.Id);

            items = ordered.Skip((page - 1) * perPage).Take(perPage).ToList();
        }
        else
        {
            items = await query
                .Include(x => x.Video).Include(x => x.MediaFile).Include(x => x.VoiceRecord)
                .Include(x => x.AdditionalLessons)
                .ApplyListSort(request, x => x.Title, x => x.SortOrder)
                .Skip((page - 1) * perPage).Take(perPage)
                .ToListAsync(ct);
        }

        var data = items.Select(ToResponse).ToList();
        return new PagedResponse<LessonContentResponse>(data, page, Paging.LastPage(total, perPage), perPage, total);
    }

    private static int EffectiveSort(LessonContent content, long lessonId) =>
        content.LessonId == lessonId
            ? content.SortOrder
            : content.AdditionalLessons.FirstOrDefault(l => l.LessonId == lessonId)?.SortOrder ?? content.SortOrder;

    public async Task<LessonContentResponse> GetAsync(long id, long? ownerId = null, CancellationToken ct = default)
    {
        var content = await WithMedia().FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Lesson content not found: {id}");

        await EnsureAccessibleAsync(content, ownerId, ct);
        return ToResponse(content);
    }

    public async Task<UploadUrlResponse> CreateUploadUrlAsync(
        LessonContentUploadUrlRequest request, long? ownerId = null, CancellationToken ct = default)
    {
        await EnsureLessonAccessibleAsync(request.LessonId, ownerId, ct);

        var folder = await FolderForAsync(request.Type, await LessonTenantAsync(request.LessonId, ct), ct);
        var path = await storage.BuildUniquePathAsync(folder, request.FileName, ct);

        var uploadUrl = storage.CreatePresignedPutUrl(path, request.ContentType, UploadUrlExpiry)
            ?? throw new ArgumentException("Presigned upload is not supported by the current storage provider");

        return new UploadUrlResponse(uploadUrl, path);
    }

    private static readonly TimeSpan MultipartUrlExpiry = TimeSpan.FromHours(6);

    public async Task<MultipartTicket?> StartMultipartAsync(LessonContentUploadUrlRequest request, long? ownerId = null, CancellationToken ct = default)
    {
        await EnsureLessonAccessibleAsync(request.LessonId, ownerId, ct);

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

        // Teacher yalnız öz və ya adminin dərsinə material əlavə edə bilər.
        if (request.UserId is long ownerId) await EnsureLessonAccessibleAsync(request.LessonId, ownerId, ct);

        var path = await ResolvePathAsync(request, ct);

        var additionalLessons = await ResolveAdditionalLessonsAsync(
            request.LessonIds, request.LessonId, tenantId, request.UserId, ct);

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

        var lessonSorts = ParseLessonSorts(request.LessonSorts);
        foreach (var lessonId in additionalLessons)
        {
            content.AdditionalLessons.Add(new LessonContentLesson
            {
                LessonId = lessonId,
                TenantId = tenantId,
                SortOrder = lessonSorts.GetValueOrDefault(lessonId, request.SortOrder)
            });
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
        long id, LessonContentAttachRequest request, long? ownerId = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Path))
            throw new ArgumentException("path is required");

        var content = await db.LessonContents
            .Include(x => x.Video).Include(x => x.MediaFile).Include(x => x.VoiceRecord)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Lesson content not found: {id}");

        EnsureOwned(content, ownerId);

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

        return await GetAsync(id, ownerId, ct);
    }

    public async Task<LessonContentResponse> UpdateAsync(
        long id, LessonContentUpdateRequest request, long? ownerId = null, CancellationToken ct = default)
    {
        var content = await db.LessonContents.IgnoreQueryFilters(new[] { "SoftDelete" })
            .Include(x => x.Video).Include(x => x.MediaFile).Include(x => x.VoiceRecord)
            .Include(x => x.AdditionalLessons)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Lesson content not found: {id}");

        EnsureOwned(content, ownerId);

        var additionalLessons = await ResolveAdditionalLessonsAsync(
            request.LessonIds, content.LessonId, content.TenantId, content.UserId, ct);
        var previousLinks = content.AdditionalLessons.ToList();
        content.AdditionalLessons.Clear();
        db.LessonContentLessons.RemoveRange(previousLinks);

        var lessonSorts = ParseLessonSorts(request.LessonSorts);
        foreach (var lessonId in additionalLessons)
        {
            content.AdditionalLessons.Add(new LessonContentLesson
            {
                LessonId = lessonId,
                TenantId = content.TenantId,
                SortOrder = lessonSorts.GetValueOrDefault(lessonId, content.SortOrder)
            });
        }

        content.Title = request.Title;
        content.Description = request.Description;
        content.SortOrder = request.SortOrder;
        content.IsActive = request.IsActive;
        await db.SaveChangesAsync(ct);

        return ToResponse(content);
    }

    public async Task DeleteAsync(long id, long? ownerId = null, CancellationToken ct = default)
    {
        var content = await db.LessonContents.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Lesson content not found: {id}");

        EnsureOwned(content, ownerId);

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

    private async Task EnsureLessonAccessibleAsync(long lessonId, long? ownerId, CancellationToken ct)
    {
        var lesson = await db.Lessons.AsNoTracking()
            .Where(x => x.Id == lessonId)
            .Select(x => new { x.UserId, IsShared = x.Subject!.IsShared })
            .FirstOrDefaultAsync(ct)
            ?? throw new ArgumentException($"Lesson not found: {lessonId}");

        if (ownerId is long id && lesson.UserId != id && !lesson.IsShared)
        {
            throw new ForbiddenException("You do not have access to this lesson");
        }
    }

    /// <summary>Oxumaq üçün: öz materialı və ya paylaşılan fənnin dərsinə bağlı material.</summary>
    private async Task EnsureAccessibleAsync(LessonContent content, long? ownerId, CancellationToken ct)
    {
        if (ownerId is not long id || content.UserId == id) return;

        var lessonIds = content.AdditionalLessons.Select(x => x.LessonId)
            .Append(content.LessonId)
            .Distinct()
            .ToList();

        if (!await db.Lessons.AsNoTracking().AnyAsync(x => lessonIds.Contains(x.Id) && x.Subject!.IsShared, ct))
        {
            throw new ForbiddenException("You do not have access to this material");
        }
    }

    /// <summary>Dəyişmək üçün yalnız sahibi olduğu material.</summary>
    private static void EnsureOwned(LessonContent content, long? ownerId)
    {
        if (ownerId.HasValue && content.UserId != ownerId)
        {
            throw new ForbiddenException("You can only modify your own material");
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
            c.AdditionalLessons.Select(l => l.LessonId).ToList(),
            c.AdditionalLessons.Count == 0
                ? null
                : c.AdditionalLessons.ToDictionary(l => l.LessonId, l => l.SortOrder));

    /// <summary>Əlavə dərslər üçün göndərilən sıra JSON-u: {"lessonId": sortOrder}.</summary>
    private static Dictionary<long, int> ParseLessonSorts(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new Dictionary<long, int>();

        try
        {
            return JsonSerializer.Deserialize<Dictionary<long, int>>(json) ?? new Dictionary<long, int>();
        }
        catch (JsonException)
        {
            throw new ArgumentException("lesson_sorts is invalid");
        }
    }

    /// <summary>Əlavə dərsləri yoxlayır: mövcud olmalı və materialın tenant-ına aid olmalıdır.</summary>
    private async Task<List<long>> ResolveAdditionalLessonsAsync(
        IEnumerable<long>? lessonIds, long primaryLessonId, long? tenantId, long? ownerId, CancellationToken ct)
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

        if (ownerId.HasValue)
        {
            var accessible = await db.Lessons.AsNoTracking()
                .Where(x => wanted.Contains(x.Id) && (x.UserId == ownerId || x.Subject!.IsShared))
                .Select(x => x.Id)
                .ToListAsync(ct);

            if (accessible.Count != wanted.Count)
            {
                throw new ForbiddenException("You do not have access to some of the selected lessons");
            }
        }

        return found.Select(x => x.Id).ToList();
    }

    public async Task BulkAsync(BulkRequest request, long? ownerId = null, CancellationToken ct = default)
    {
        if (ownerId.HasValue) await EnsureAllOwnedAsync(request.Ids, ownerId.Value, ct);
        await BulkHelper.ApplyAsync<LessonContent>(db, request, ct);
    }

    public async Task ReorderAsync(SortRequest request, long? ownerId = null, CancellationToken ct = default)
    {
        if (ownerId.HasValue) await EnsureAllOwnedAsync(request.Ids, ownerId.Value, ct);
        await SortHelper.ReorderAsync<LessonContent>(db, request, ct);
    }

    private async Task EnsureAllOwnedAsync(IEnumerable<long> ids, long ownerId, CancellationToken ct)
    {
        var wanted = ids.Distinct().ToList();
        var owned = await db.LessonContents.Where(x => wanted.Contains(x.Id) && x.UserId == ownerId)
            .Select(x => x.Id).ToListAsync(ct);

        if (owned.Count != wanted.Count) throw new ForbiddenException("You can only modify your own materials");
    }
}
