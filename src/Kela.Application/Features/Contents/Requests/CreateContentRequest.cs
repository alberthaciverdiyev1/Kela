using Kela.Domain.Enums;

namespace Kela.Application.Features.Contents.Requests;

public sealed record CreateContentRequest(
    int TeacherId,
    string Title,
    string? Description,
    ContentType Type,
    string? Url,
    int? ParentId);
