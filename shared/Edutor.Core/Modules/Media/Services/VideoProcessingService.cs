using Edutor.Core.Data;
using Edutor.Core.Modules.Media.Abstractions;
using Edutor.Core.Modules.Media.Entities;
using Edutor.Core.Modules.Media.Requests;
using Microsoft.EntityFrameworkCore;

namespace Edutor.Core.Modules.Media.Services;

public sealed class VideoProcessingService(AppDbContext db) : IVideoProcessingService
{
    public async Task<bool> MarkProcessingAsync(string sourcePath, CancellationToken ct = default)
    {
        var video = await FindAsync(sourcePath, ct);
        if (video is null) return false;

        video.ConversionStatus = VideoStatus.Processing;
        video.ConversionError = null;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> MarkProcessedAsync(VideoProcessedRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.SourcePath))
            throw new ArgumentException("sourcePath is required", nameof(request));

        var video = await FindAsync(request.SourcePath, ct);
        if (video is null) return false;

        return await ApplyAsync(video, request, ct);
    }

    public async Task<bool> MarkProcessedAsync(long videoId, VideoProcessedRequest request, CancellationToken ct = default)
    {
        var video = await db.Videos.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == videoId && x.DeletedAt == null, ct);

        if (video is null) return false;

        return await ApplyAsync(video, request, ct);
    }

    private async Task<bool> ApplyAsync(Video video, VideoProcessedRequest request, CancellationToken ct)
    {
        if (request.Status is not (VideoStatus.Ready or VideoStatus.Failed))
            throw new ArgumentException($"status must be '{VideoStatus.Ready}' or '{VideoStatus.Failed}'", nameof(request));

        var failed = request.Status == VideoStatus.Failed;

        // Uğursuz konversiyada mövcud m3u8 silinmir — köhnə versiya işlək qalsın.
        if (!failed)
        {
            video.PlaybackUrl = string.IsNullOrWhiteSpace(request.PlaybackUrl) ? video.PlaybackUrl : request.PlaybackUrl;
            video.Thumbnail = string.IsNullOrWhiteSpace(request.Thumbnail) ? video.Thumbnail : request.Thumbnail;

            if (request.DurationSeconds is > 0) video.DurationSeconds = request.DurationSeconds;
        }

        video.ConversionStatus = request.Status;
        video.ConversionError = failed ? Truncate(request.Error, 500) : null;

        await db.SaveChangesAsync(ct);
        return true;
    }

    /// <summary>
    /// Worker heç bir tenant konteksti göndərmir; tenant məlum olmadıqda qlobal filtr
    /// tətbiq olunmur, ona görə video bütün tenantlar üzrə tapılır.
    /// </summary>
    private Task<Video?> FindAsync(string sourcePath, CancellationToken ct)
        => db.Videos.OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(x => x.Path == sourcePath, ct);

    private static string? Truncate(string? value, int max)
        => value is null || value.Length <= max ? value : value[..max];
}
