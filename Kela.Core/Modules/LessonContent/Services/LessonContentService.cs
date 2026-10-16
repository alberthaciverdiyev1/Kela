using Kela.Core.Common.Responses;
using Kela.Core.Common.MultiTenancy;
using Kela.Core.Common.Storage;
using Kela.Core.Common.Bulk;
using Kela.Core.Common.Sorting;
using Kela.Core.Data;
using Kela.Core.Modules.LessonContent.Requests;
using Kela.Core.Modules.LessonContent.Responses;
using Microsoft.EntityFrameworkCore;

namespace Kela.Core.Modules.LessonContent.Services;

using LessonContent = Kela.Core.Modules.LessonContent.Entities.LessonContent;
using LessonContentType = Kela.Core.Modules.LessonContent.Entities.LessonContentType;
using MediaFile = Kela.Core.Modules.Media.Entities.MediaFile;
using Video = Kela.Core.Modules.Media.Entities.Video;
using VoiceRecord = Kela.Core.Modules.Media.Entities.VoiceRecord;

public sealed class LessonContentService(AppDbContext db, IFileStorage storage, ITenantProvider tenants)
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
            query = query.Where(x => x.LessonId == request.LessonId);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = $"%{request.Search.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.Title, term));
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
        var folder = await FolderForAsync(request.Type, await LessonTenantAsync(request.LessonId, ct), ct);
        var path = $"{folder}/{Guid.NewGuid():N}{extension}";

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
        var content = await db.LessonContents.IgnoreQueryFilters(new[] { "SoftDelete" })
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

        throw new ArgumentException("File or Path is required");
    }

    /// <summary>
    /// Bucket quruluşu tenant üzrə ayrılır:
    /// <c>{tenant}/videos/raw</c>, <c>{tenant}/videos/m3u8</c>, <c>{tenant}/voices</c>, <c>{tenant}/files</c>.
    /// Konversiyadan sonra worker m3u8-i <c>{tenant}/videos/m3u8</c> altına yazır və həmin
    /// açarı callback ilə bildirir.
    /// </summary>
    private async Task<string> FolderForAsync(LessonContentType type, long? fallbackTenantId, CancellationToken ct)
    {
        var prefix = await TenantFolderAsync(fallbackTenantId, ct);

        return type switch
        {
            LessonContentType.Video => $"{prefix}/videos/raw",
            LessonContentType.Voice => $"{prefix}/voices",
            _ => $"{prefix}/files"
        };
    }

    /// <summary>
    /// Qovluq adı kimi tenant-ın slug-ı işlədilir (ad boşluq və Azərbaycan hərfləri
    /// daşıya bilər, slug isə URL-təhlükəsizdir). Slug boşdursa ad sadələşdirilir.
    /// </summary>
    private async Task<string> TenantFolderAsync(long? fallbackTenantId, CancellationToken ct)
    {
        // Superadmin tenant-a keçməyibsə sorğuda tenant olmur; o halda məzmunun aid
        // olduğu dərsin tenant-ını götürürük — fayl "shared" qovluğuna düşməsin.
        var id = tenants.TenantId is > 0 ? tenants.TenantId : fallbackTenantId;
        if (id is not > 0) return "shared";

        var tenant = await db.Tenants.AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new { t.Name, t.Slug })
            .FirstOrDefaultAsync(ct);

        if (tenant is null) return $"tenant-{id}";
        if (!string.IsNullOrWhiteSpace(tenant.Slug)) return Sanitize(tenant.Slug);

        var fromName = Sanitize(tenant.Name);
        return fromName.Length > 0 ? fromName : $"tenant-{id}";
    }

    private static string Sanitize(string value)
    {
        var lowered = value.Trim().ToLowerInvariant();
        var sb = new System.Text.StringBuilder(lowered.Length);

        foreach (var ch in lowered)
        {
            sb.Append(ch is >= 'a' and <= 'z' or >= '0' and <= '9' ? ch : '-');
        }

        var result = sb.ToString().Trim('-');
        while (result.Contains("--")) result = result.Replace("--", "-");
        return result;
    }

    private IQueryable<LessonContent> WithMedia() =>
        db.LessonContents.AsNoTracking()
            .Include(x => x.Video).Include(x => x.MediaFile).Include(x => x.VoiceRecord);

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
            duration, thumbnailUrl, conversionStatus);

    public Task BulkAsync(BulkRequest request, CancellationToken ct = default)
        => BulkHelper.ApplyAsync<LessonContent>(db, request, ct);

    public Task ReorderAsync(SortRequest request, CancellationToken ct = default)
        => SortHelper.ReorderAsync<LessonContent>(db, request, ct);
}
