using System.Text;
using System.Text.Encodings.Web;

namespace Kela.Web.Shared.Navigation;

public static class PagerHtml
{
    public static string Render(int page, int totalPages, string? prevText, string? nextText)
    {
        if (totalPages <= 1)
        {
            return "";
        }

        var builder = new StringBuilder();
        builder.Append("<div class=\"flex items-center justify-end gap-1 border-t border-base-300 px-4 py-3.5\">");

        builder.Append($"<button type=\"button\" class=\"pager-btn btn btn-sm btn-ghost\"{(page <= 1 ? " disabled" : "")} data-page=\"{page - 1}\">");
        builder.Append(HtmlEncoder.Default.Encode(string.IsNullOrEmpty(prevText) ? "‹" : prevText));
        builder.Append("</button>");

        for (var p = 1; p <= totalPages; p++)
        {
            if (p == page)
            {
                builder.Append($"<span class=\"pager-btn btn btn-sm btn-primary\">{p}</span>");
            }
            else if (p <= 2 || p >= totalPages - 1 || Math.Abs(p - page) <= 1)
            {
                builder.Append($"<button type=\"button\" class=\"pager-btn btn btn-sm btn-ghost\" data-page=\"{p}\">{p}</button>");
            }
            else if (p == 3 || p == totalPages - 2)
            {
                builder.Append("<span class=\"px-1 text-base-content/40\">…</span>");
            }
        }

        builder.Append($"<button type=\"button\" class=\"pager-btn btn btn-sm btn-ghost\"{(page >= totalPages ? " disabled" : "")} data-page=\"{page + 1}\">");
        builder.Append(HtmlEncoder.Default.Encode(string.IsNullOrEmpty(nextText) ? "›" : nextText));
        builder.Append("</button>");

        builder.Append("</div>");
        return builder.ToString();
    }
}
