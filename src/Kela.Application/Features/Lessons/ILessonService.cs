using Kela.Application.Features.Lessons.Responses;

namespace Kela.Application.Features.Lessons;

public interface ILessonService
{
    Task<LessonResponse?> GetByContentIdAsync(int contentId, CancellationToken cancellationToken = default);
    Task SetVideoAsync(int contentId, string videoPath, string thumbnailPath, int durationSeconds, CancellationToken cancellationToken = default);
    Task UpdateOrderAsync(int contentId, int orderIndex, CancellationToken cancellationToken = default);
}
