namespace Edutor.Core.Modules.Notification.Requests;

public class SaveNotificationTokenRequest
{
    public string Token { get; set; } = string.Empty;

    public string? DeviceType { get; set; }
}
