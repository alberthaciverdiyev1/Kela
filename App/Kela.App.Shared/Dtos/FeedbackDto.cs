namespace Kela.App.Shared.Dtos;

public record FeedbackDto(
    long Id,
    string UserName,
    string Message,
    string Status,
    DateTimeOffset CreatedAt);
