namespace Edutor.App.Shared.Services;

/// <summary>Cihazın push üçün identifikatoru (token) və növü (ios | android | web).</summary>
public readonly record struct DeviceToken(string Token, string DeviceType);

/// <summary>
/// Cihazın push tokenini təmin edir.
/// Web: brauzer sessiyasına bağlı sabit identifikator (deviceType = "web").
/// Native (gələcək MAUI app): FCM/APNs tokeni ilə əvəz olunacaq.
/// </summary>
public interface IDeviceTokenProvider
{
    ValueTask<DeviceToken?> GetAsync();
}
