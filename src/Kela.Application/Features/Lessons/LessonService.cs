using Kela.Application.Features.Lessons.Responses;
using Kela.Application.Patterns;
using Kela.Domain.Entities;

namespace Kela.Application.Features.Lessons;

internal sealed class LessonService(ILessonRepository lessons, IUnitOfWork unitOfWork) : ILessonService
{
    public async Task<LessonResponse?> GetByContentIdAsync(int contentId, CancellationToken cancellationToken = default)
    {
        var lesson = await lessons.GetByContentIdAsync(contentId, cancellationToken);
        if (lesson is null || lesson.Content is null)
        {
            return null;
        }

        return new LessonResponse(
            lesson.ContentId,
            lesson.TeacherId,
            lesson.Content.Title,
            lesson.Content.Description,
            lesson.VideoPath,
            lesson.ThumbnailPath,
            lesson.DurationSeconds,
            lesson.IsPublished,
            lesson.OrderIndex);
    }

    public async Task SetVideoAsync(int contentId, string videoPath, string thumbnailPath, int durationSeconds, CancellationToken cancellationToken = default)
    {
        var lesson = await lessons.GetByContentIdAsync(contentId, cancellationToken)
            ?? throw new KeyNotFoundException($"ContentId = {contentId} olan ders bulunamadı.");

        lesson.VideoPath = videoPath;
        lesson.ThumbnailPath = thumbnailPath;
        lesson.DurationSeconds = durationSeconds;
        lesson.UpdatedAt = DateTime.UtcNow;

        lessons.Update(lesson);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateOrderAsync(int contentId, int orderIndex, CancellationToken cancellationToken = default)
    {
        var lesson = await lessons.GetByContentIdAsync(contentId, cancellationToken)
            ?? throw new KeyNotFoundException($"ContentId = {contentId} olan ders bulunamadı.");

        lesson.OrderIndex = orderIndex;
        lesson.UpdatedAt = DateTime.UtcNow;

        lessons.Update(lesson);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
