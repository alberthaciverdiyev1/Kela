using Ganss.Xss;

namespace Kela.Core.Common.Content;

/// <summary>
/// İstifadəçi/admin tərəfindən yazılan HTML-i (siyasət, istifadə şərtləri)
/// təhlükəsiz formaya salır — script, iframe, on* atributları və s. silinir.
/// </summary>
public static class SafeHtml
{
    public static string? Sanitize(string? html)
    {
        if (string.IsNullOrWhiteSpace(html)) return html;

        // HtmlSanitizer nümunəsi thread-safe deyil — hər çağırışda yenisini qururuq.
        return new HtmlSanitizer().Sanitize(html);
    }
}
