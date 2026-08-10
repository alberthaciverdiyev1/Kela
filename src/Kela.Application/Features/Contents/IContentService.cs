using Kela.Application.Features.Contents.Requests;
using Kela.Application.Features.Contents.Responses;

namespace Kela.Application.Features.Contents;

public interface IContentService
{
    Task<List<ContentResponse>> GetByTeacherAsync(int teacherId, CancellationToken cancellationToken = default);
    Task<ContentResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(CreateContentRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, UpdateContentRequest request, CancellationToken cancellationToken = default);
    Task SetPublishedAsync(int id, bool published, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
