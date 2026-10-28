using Edutor.Core.Data;
using Edutor.Core.Modules.Media.Abstractions;
using Edutor.Core.Modules.Media.Entities;
using Edutor.Core.Modules.Media.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Edutor.Tests.Media;

[Collection("api")]
public class VideoProcessingTests(ApiFactory factory)
{
    private static string Unique(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

    private async Task<T> WithServiceAsync<T>(Func<IVideoProcessingService, Task<T>> action)
    {
        using var scope = factory.Services.CreateScope();
        return await action(scope.ServiceProvider.GetRequiredService<IVideoProcessingService>());
    }

    private async Task<(long ContentId, string Path)> CreateVideoAsync()
    {
        var subjectId = await TestData.SubjectAsync(factory, Unique("Subject"));
        var lessonId = await TestData.LessonAsync(factory, subjectId, Unique("Lesson"));
        var contentId = await TestData.LessonContentAsync(factory, lessonId, Unique("Video"));

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var path = await db.LessonContents.Where(c => c.Id == contentId)
            .Select(c => c.Video!.Path).FirstAsync();

        return (contentId, path);
    }

    private async Task<Video> ReadVideoAsync(long contentId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.LessonContents.Where(c => c.Id == contentId)
            .Select(c => c.Video!).FirstAsync();
    }

    [Fact]
    public async Task Video_starts_as_pending()
    {
        var (contentId, _) = await CreateVideoAsync();
        var video = await ReadVideoAsync(contentId);

        Assert.Equal(VideoStatus.Pending, video.ConversionStatus);
        Assert.Null(video.PlaybackUrl);
    }

    [Fact]
    public async Task Callback_marks_video_ready_by_source_path()
    {
        var (contentId, path) = await CreateVideoAsync();

        var ok = await WithServiceAsync(s => s.MarkProcessedAsync(new VideoProcessedRequest
        {
            SourcePath = path,
            PlaybackUrl = "video/hls/master.m3u8",
            Thumbnail = "video/hls/thumb.jpg",
            DurationSeconds = 42,
            Status = VideoStatus.Ready
        }));

        Assert.True(ok);

        var video = await ReadVideoAsync(contentId);
        Assert.Equal(VideoStatus.Ready, video.ConversionStatus);
        Assert.Equal("video/hls/master.m3u8", video.PlaybackUrl);
        Assert.Equal("video/hls/thumb.jpg", video.Thumbnail);
        Assert.Equal(42, video.DurationSeconds);
        Assert.Null(video.ConversionError);
    }

    [Fact]
    public async Task Unknown_source_path_reports_not_found_so_the_queue_retries()
    {
        var ok = await WithServiceAsync(s => s.MarkProcessedAsync(new VideoProcessedRequest
        {
            SourcePath = "video/does-not-exist.mp4",
            PlaybackUrl = "video/hls/master.m3u8",
            Status = VideoStatus.Ready
        }));

        Assert.False(ok);
    }

    [Fact]
    public async Task Failed_conversion_keeps_the_previous_playback_url()
    {
        var (contentId, path) = await CreateVideoAsync();

        await WithServiceAsync(s => s.MarkProcessedAsync(new VideoProcessedRequest
        {
            SourcePath = path,
            PlaybackUrl = "video/hls/master.m3u8",
            Status = VideoStatus.Ready
        }));

        // İkinci cəhd uğursuz olur — köhnə işlək m3u8 silinməməlidir.
        await WithServiceAsync(s => s.MarkProcessedAsync(new VideoProcessedRequest
        {
            SourcePath = path,
            Status = VideoStatus.Failed,
            Error = "ffmpeg crashed"
        }));

        var video = await ReadVideoAsync(contentId);
        Assert.Equal(VideoStatus.Failed, video.ConversionStatus);
        Assert.Equal("video/hls/master.m3u8", video.PlaybackUrl);
        Assert.Equal("ffmpeg crashed", video.ConversionError);
    }

    [Fact]
    public async Task Invalid_status_is_rejected()
    {
        var (_, path) = await CreateVideoAsync();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            WithServiceAsync(s => s.MarkProcessedAsync(new VideoProcessedRequest
            {
                SourcePath = path,
                Status = "whatever"
            })));
    }
}
