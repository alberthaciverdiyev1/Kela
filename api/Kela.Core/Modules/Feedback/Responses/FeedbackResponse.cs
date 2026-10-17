namespace Kela.Core.Modules.Feedback.Responses;

public record FeedbackMessageResponse(
    long Id,
    string SenderRole,
    string SenderName,
    string Body,
    string? ImageUrl,
    bool IsRead,
    DateTimeOffset CreatedAt);

public record FeedbackThreadResponse(
    long Id,
    long? UserId,
    string UserName,
    string UserEmail,
    string Role,
    DateTimeOffset LastMessageAt,
    bool HasUnreadForAdmin,
    IReadOnlyList<FeedbackMessageResponse> Messages);
