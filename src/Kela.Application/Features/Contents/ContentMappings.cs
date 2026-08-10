using Kela.Application.Features.Contents.Responses;
using Kela.Domain.Entities;

namespace Kela.Application.Features.Contents;

public static class ContentMappings
{
    public static ContentResponse ToResponse(this Content content) => new(
        content.Id,
        content.TeacherId,
        content.Title,
        content.Description,
        content.Type,
        content.Url,
        content.IsPublished,
        content.CreatedAt);
}
