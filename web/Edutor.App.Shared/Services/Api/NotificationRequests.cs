using Edutor.App.Shared.Dtos.Notification;

namespace Edutor.App.Shared.Services.Api;

public sealed partial class AppApi
{
    // --- Bildirişlər ---
    public Task<PagedResult<NotificationDto>> NotificationsAsync(int page = 1, int perPage = 20, string? search = null)
    {
        var query = new List<string> { $"page={page}", $"perPage={perPage}" };
        if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search)}");
        return _api.GetAsync<PagedResult<NotificationDto>>("/api/student/notifications?" + string.Join("&", query));
    }

    /// <summary>Cihazın push tokenini serverdə qeydə alır (FCM/APNs).</summary>
    public Task<string> SaveNotificationTokenAsync(string token, string? deviceType)
        => _api.PostAsync<string>("/api/student/notifications/token", new { token, deviceType });

    /// <summary>Brauzer abunəliyinin VAPID public key-i (provayder qurulmayıbsa boş).</summary>
    public Task<string> WebPushKeyAsync()
        => _api.GetAsync<string>("/api/student/notifications/web-push");

    /// <summary>Web Push abunəlik JSON-u uzundur — silmə body ilə göndərilir.</summary>
    public Task<string> RemoveNotificationTokenByBodyAsync(string token)
        => _api.PostAsync<string>("/api/student/notifications/token/remove", new { token });
}
