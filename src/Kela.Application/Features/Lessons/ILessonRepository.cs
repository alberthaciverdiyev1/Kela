using Kela.Domain.Entities;

namespace Kela.Application.Features.Lessons;

public interface ILessonRepository
{
    Task<Lesson?> GetByContentIdAsync(int contentId, CancellationToken cancellationToken = default);
    void Add(Lesson lesson);
    void Update(Lesson lesson);
    void Remove(Lesson lesson);
}
