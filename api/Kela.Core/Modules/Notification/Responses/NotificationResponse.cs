namespace Kela.Core.Modules.Notification.Responses;

public record NotificationResponse(
    long Id,
    string Title,
    string? Body,
    string? Image,
    bool All,
    DateTimeOffset CreatedAt);
