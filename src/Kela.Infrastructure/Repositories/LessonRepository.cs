using Kela.Application.Features.Lessons;
using Kela.Domain.Entities;
using Kela.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kela.Infrastructure.Repositories;

internal sealed class LessonRepository(KelaDbContext context) : ILessonRepository
{
    public async Task<Lesson?> GetByContentIdAsync(int contentId, CancellationToken cancellationToken = default)
        => await context.Lessons
            .Include(l => l.Content)
            .FirstOrDefaultAsync(l => l.ContentId == contentId, cancellationToken);

    public void Add(Lesson lesson) => context.Lessons.Add(lesson);

    public void Update(Lesson lesson) => context.Lessons.Update(lesson);

    public void Remove(Lesson lesson) => context.Lessons.Remove(lesson);
}
