using Kela.Domain.Enums;

namespace Kela.Application.Features.Nodes.Responses;

public sealed record LessonSummaryResponse(
    bool HasVideo,
    int DurationSeconds);

public sealed record ContentSummaryResponse(
    int Id,
    string Title,
    string? Description,
    ContentType Type,
    string? Url,
    bool IsPublished,
    LessonSummaryResponse? Lesson = null);

public sealed record NodeResponse(
    int Id,
    string Name,
    NodeType Kind,
    int Position,
    int? ParentId,
    int? ContentId,
    ContentSummaryResponse? Content,
    IReadOnlyList<NodeResponse> Children);
