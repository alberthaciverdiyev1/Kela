using Kela.Domain.Entities;

namespace Kela.Application.Features.Contents;

public interface IContentRepository
{
    Task<List<Content>> GetByTeacherAsync(int teacherId, CancellationToken cancellationToken = default);
    Task<Content?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    void Add(Content content);
    void Update(Content content);
    void Remove(Content content);
}
