using Kela.Domain.Enums;

namespace Kela.Application.Features.Contents.Responses;

public sealed record ContentResponse(
    int Id,
    int TeacherId,
    string Title,
    string? Description,
    ContentType Type,
    string? Url,
    bool IsPublished,
    DateTime CreatedAt);
