namespace Edutor.Core.Modules.SystemNotification.Responses;

public record SystemNotificationResponse(long Id, string Title, string? Body, bool IsRead, DateTimeOffset CreatedAt);
