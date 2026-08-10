namespace Kela.Application.Features.Contents.Requests;

public sealed record UpdateContentRequest(
    string Title,
    string? Description,
    string? Url);
