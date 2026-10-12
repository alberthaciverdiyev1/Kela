namespace LanguageLMS.Api.Common.Extensions;

public static class HttpRequestExtensions
{
    /// <summary>
    /// Nisbi fayl URL-ini cari sorğunun sxem+host-u ilə absolute edir.
    /// Artıq http(s) ilə başlayırsa (məs. R2 PublicBaseUrl) toxunmadan qaytarır.
    /// </summary>
    public static string? ToAbsoluteUrl(this HttpRequest request, string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return url;

        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return url;
        }

        return $"{request.Scheme}://{request.Host}/{url.TrimStart('/')}";
    }
}
