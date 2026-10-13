using LanguageLMS.App.Shared.Dtos.Notification;

namespace LanguageLMS.App.Shared.Services.Api;

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

    /// <summary>Cihazın push tokenini serverdən silir.</summary>
    public Task<string> RemoveNotificationTokenAsync(string token)
        => _api.DeleteAsync<string>($"/api/student/notifications/token?token={Uri.EscapeDataString(token)}");
}
