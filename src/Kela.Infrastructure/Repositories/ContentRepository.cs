using Kela.Application.Features.Contents;
using Kela.Domain.Entities;
using Kela.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kela.Infrastructure.Repositories;

internal sealed class ContentRepository(KelaDbContext context) : IContentRepository
{
    public async Task<List<Content>> GetByTeacherAsync(int teacherId, CancellationToken cancellationToken = default)
        => await context.Contents
            .Where(c => c.TeacherId == teacherId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<Content?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await context.Contents.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public void Add(Content content) => context.Contents.Add(content);

    public void Update(Content content) => context.Contents.Update(content);

    public void Remove(Content content) => context.Contents.Remove(content);
}
