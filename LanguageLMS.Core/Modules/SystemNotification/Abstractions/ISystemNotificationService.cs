using LanguageLMS.Core.Modules.SystemNotification.Responses;

namespace LanguageLMS.Core.Modules.SystemNotification.Abstractions;

public interface ISystemNotificationService
{
    Task<IReadOnlyList<SystemNotificationResponse>> ListAsync(long recipientId, CancellationToken ct = default);

    Task<SystemNotificationResponse> CreateAsync(long recipientId, string title, string? body, CancellationToken ct = default);

    Task MarkReadAsync(long recipientId, long id, CancellationToken ct = default);

    Task MarkAllReadAsync(long recipientId, CancellationToken ct = default);
}
