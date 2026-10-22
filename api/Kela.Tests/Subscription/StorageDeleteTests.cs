using Kela.Core.Common.Bulk;
using Kela.Core.Data;
using Kela.Core.Modules.LessonContent.Abstractions;
using Kela.Core.Modules.LessonContent.Requests;
using Kela.Core.Modules.LessonContent.Responses;
using Kela.Core.Modules.Media.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kela.Tests.Subscription;

/// <summary>
/// Material silinəndə fayllar da silinir və depolama sayğacı azalır — istifadəçi
/// köhnə videonu silib yerinə yenisini yükləyə bilsin.
/// </summary>
[Collection("api")]
public class StorageDeleteTests(ApiFactory factory)
{
    private async Task<T> With<TService, T>(Func<TService, Task<T>> action) where TService : notnull
    {
        using var scope = factory.Services.CreateScope();
        return await action(scope.ServiceProvider.GetRequiredService<TService>());
    }

    private async Task<long> UsedBytesAsync()
        => await With<AppDbContext, long>(async db =>
            await db.TenantUsages.IgnoreQueryFilters()
                .Where(x => x.TenantId == 0)
                .OrderByDescending(x => x.Id)
                .Select(x => x.StorageBytes)
                .FirstOrDefaultAsync());

    private async Task<(long ContentId, string Path)> CreateContentAsync()
    {
        var subjectId = await TestData.SubjectAsync(factory, $"S-{Guid.NewGuid():N}");
        var lessonId = await TestData.LessonAsync(factory, subjectId, $"L-{Guid.NewGuid():N}");
        var contentId = await TestData.LessonContentAsync(factory, lessonId, $"C-{Guid.NewGuid():N}");

        var path = await With<AppDbContext, string>(async db =>
            await db.LessonContents.Where(x => x.Id == contentId).Select(x => x.Video!.Path).FirstAsync());

        return (contentId, path);
    }

    [Fact]
    public async Task Creating_content_adds_to_storage_and_deleting_takes_it_back()
    {
        var before = await UsedBytesAsync();

        var (contentId, _) = await CreateContentAsync();
        var afterCreate = await UsedBytesAsync();

        Assert.True(afterCreate > before, $"Yükləmə sayğaca əlavə olunmadı: {before} → {afterCreate}");

        await With<ILessonContentService, bool>(async s =>
        {
            await s.DeleteAsync(contentId);
            return true;
        });

        var afterDelete = await UsedBytesAsync();

        Assert.Equal(before, afterDelete);
    }

    [Fact]
    public async Task Deleting_content_moves_the_record_to_trash()
    {
        var (contentId, _) = await CreateContentAsync();

        await With<ILessonContentService, bool>(async s =>
        {
            await s.DeleteAsync(contentId);
            return true;
        });

        var deletedAt = await With<AppDbContext, DateTimeOffset?>(async db =>
            await db.LessonContents.IgnoreQueryFilters(new[] { "SoftDelete" })
                .Where(x => x.Id == contentId)
                .Select(x => x.DeletedAt)
                .FirstAsync());

        Assert.NotNull(deletedAt);
    }

    [Fact]
    public async Task Bulk_delete_frees_storage_as_well()
    {
        var before = await UsedBytesAsync();

        var (contentId, _) = await CreateContentAsync();
        Assert.True(await UsedBytesAsync() > before);

        await With<ILessonContentService, bool>(async s =>
        {
            await s.BulkAsync(new BulkRequest { Ids = [contentId], Action = BulkActions.Delete });
            return true;
        });

        Assert.Equal(before, await UsedBytesAsync());
    }

    [Fact]
    public async Task Deleting_content_removes_the_file_from_storage()
    {
        var (contentId, path) = await CreateContentAsync();

        var storage = factory.Services.GetRequiredService<Kela.Core.Common.Storage.IFileStorage>();
        Assert.True(await storage.ExistsAsync(path));

        await With<ILessonContentService, bool>(async s =>
        {
            await s.DeleteAsync(contentId);
            return true;
        });

        Assert.False(await storage.ExistsAsync(path));
    }
}
