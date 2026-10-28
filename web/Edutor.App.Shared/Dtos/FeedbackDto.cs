namespace Edutor.App.Shared.Dtos;

public record FeedbackMessageDto(
    long Id,
    string SenderRole,
    string SenderName,
    string Body,
    string? ImageUrl,
    bool IsRead,
    DateTimeOffset CreatedAt);

public record FeedbackThreadDto(
    long Id,
    string UserName,
    string UserEmail,
    DateTimeOffset LastMessageAt,
    bool HasUnreadForAdmin,
    List<FeedbackMessageDto> Messages);
