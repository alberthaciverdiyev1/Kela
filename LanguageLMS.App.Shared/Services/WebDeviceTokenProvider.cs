using Microsoft.JSInterop;

namespace LanguageLMS.App.Shared.Services;

/// <summary>
/// Web üçün cihaz tokeni: localStorage-də saxlanan sabit identifikator.
/// Qeyd: bu, əsl FCM/APNs tokeni deyil — native app gələndə <see cref="IDeviceTokenProvider"/>
/// FCM tokeni qaytaran implementasiya ilə əvəz olunmalıdır.
/// </summary>
public sealed class WebDeviceTokenProvider(IJSRuntime js) : IDeviceTokenProvider
{
    private const string Key = "lms.deviceToken";

    public async ValueTask<DeviceToken?> GetAsync()
    {
        try
        {
            var token = await js.InvokeAsync<string?>("localStorage.getItem", Key);
            if (string.IsNullOrWhiteSpace(token))
            {
                token = "web-" + Guid.NewGuid().ToString("N");
                await js.InvokeVoidAsync("localStorage.setItem", Key, token);
            }

            return new DeviceToken(token, "web");
        }
        catch
        {
            return null;
        }
    }
}
