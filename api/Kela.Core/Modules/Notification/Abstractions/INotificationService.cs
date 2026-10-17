using Kela.Core.Common.Responses;
using Kela.Core.Modules.Notification.Requests;
using Kela.Core.Modules.Notification.Responses;

namespace Kela.Core.Modules.Notification.Abstractions;

public interface INotificationService
{
    Task<NotificationResponse> CreateAsync(CreateNotificationRequest request, long? userId, CancellationToken ct = default);

    Task<PagedResponse<NotificationResponse>> ListAsync(NotificationListRequest request, CancellationToken ct = default);

    Task DeleteAsync(long id, CancellationToken ct = default);

    Task<PagedResponse<NotificationResponse>> ListForStudentAsync(long studentId, NotificationListRequest request, CancellationToken ct = default);

    Task SaveTokenAsync(long studentId, SaveNotificationTokenRequest request, CancellationToken ct = default);

    Task DeactivateTokenAsync(long studentId, string token, CancellationToken ct = default);
}
