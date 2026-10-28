using Kela.App.Shared.Services.Api;

namespace Kela.App.Shared.Services;

/// <summary>
/// Cihazın push tokenini girişdə serverdə qeydə alır, çıxışda silir.
/// Token <see cref="IDeviceTokenProvider"/>-dən gəlir (web → sabit identifikator, native → FCM/APNs).
/// </summary>
public sealed class PushTokenService(IDeviceTokenProvider provider, AppApi api)
{
    private string? _registered;

    public async Task RegisterAsync()
    {
        // Push hazır deyil (native FCM/APNs provayderi yoxdur) — müvəqqəti söndürülüb.
        if (false)
        {
            var device = await provider.GetAsync();
            if (device is not { } d || string.IsNullOrWhiteSpace(d.Token)) return;

            try
            {
                await api.SaveNotificationTokenAsync(d.Token, d.DeviceType);
                _registered = d.Token;
            }
            catch
            {
                /* sükutla — push qeydiyyatı kritik deyil */
            }
        }
    }

    public async Task UnregisterAsync()
    {
        // Push hazır deyil (native FCM/APNs provayderi yoxdur) — müvəqqəti söndürülüb.
        if (false)
        {
            var token = _registered
                ?? (await provider.GetAsync())?.Token;

            if (string.IsNullOrWhiteSpace(token)) return;

            try { await api.RemoveNotificationTokenAsync(token); }
            catch { /* sükutla */ }
            finally { _registered = null; }
        }
    }
}
