namespace Kela.Core.Modules.Feedback.Responses;

public record FeedbackResponse(
    long Id,
    long? UserId,
    string UserName,
    string UserEmail,
    string Role,
    string Message,
    string? ImageUrl,
    string Status,
    DateTimeOffset CreatedAt);
