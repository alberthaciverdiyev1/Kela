using LanguageLMS.Core.Common.Responses;
using LanguageLMS.Core.Modules.Notification.Requests;
using LanguageLMS.Core.Modules.Notification.Responses;

namespace LanguageLMS.Core.Modules.Notification.Abstractions;

public interface INotificationService
{
    Task<NotificationResponse> CreateAsync(CreateNotificationRequest request, long? userId, CancellationToken ct = default);

    Task<PagedResponse<NotificationResponse>> ListAsync(NotificationListRequest request, CancellationToken ct = default);

    Task DeleteAsync(long id, CancellationToken ct = default);

    Task<PagedResponse<NotificationResponse>> ListForStudentAsync(long studentId, NotificationListRequest request, CancellationToken ct = default);

    Task SaveTokenAsync(long studentId, SaveNotificationTokenRequest request, CancellationToken ct = default);

    Task DeactivateTokenAsync(long studentId, string token, CancellationToken ct = default);
}
