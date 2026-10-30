using Microsoft.JSInterop;

namespace Edutor.App.Shared.Services;

// Brauzer Web Push abunəliyini (JSON) token kimi qaytarır; icazə yoxdursa null.
// Native app gələndə FCM/APNs tokeni qaytaran implementasiya ilə əvəz olunmalıdır.
public sealed class WebDeviceTokenProvider(IJSRuntime js) : IDeviceTokenProvider
{
    public async ValueTask<DeviceToken?> GetAsync()
    {
        try
        {
            var subscription = await js.InvokeAsync<string?>("edutorPush.current");
            return string.IsNullOrWhiteSpace(subscription) ? null : new DeviceToken(subscription, "web");
        }
        catch
        {
            return null;
        }
    }
}
