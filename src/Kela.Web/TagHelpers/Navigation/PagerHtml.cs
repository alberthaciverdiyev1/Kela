using System.Text;
using System.Text.Encodings.Web;

namespace Kela.Web.TagHelpers.Navigation;

public static class PagerHtml
{
    public static string Render(int page, int totalPages, string? prevText, string? nextText)
    {
        if (totalPages <= 1)
        {
            return "";
        }

        var builder = new StringBuilder();
        builder.Append("<div class=\"pagination\">");

        builder.Append($"<button type=\"button\" class=\"pager-btn\"{(page <= 1 ? " disabled" : "")} data-page=\"{page - 1}\">");
        builder.Append(HtmlEncoder.Default.Encode(string.IsNullOrEmpty(prevText) ? "‹" : prevText));
        builder.Append("</button>");

        for (var p = 1; p <= totalPages; p++)
        {
            if (p == page)
            {
                builder.Append($"<span class=\"pager-btn pager-current\">{p}</span>");
            }
            else if (p <= 2 || p >= totalPages - 1 || Math.Abs(p - page) <= 1)
            {
                builder.Append($"<button type=\"button\" class=\"pager-btn\" data-page=\"{p}\">{p}</button>");
            }
            else if (p == 3 || p == totalPages - 2)
            {
                builder.Append("<span class=\"pager-ellipsis\">…</span>");
            }
        }

        builder.Append($"<button type=\"button\" class=\"pager-btn\"{(page >= totalPages ? " disabled" : "")} data-page=\"{page + 1}\">");
        builder.Append(HtmlEncoder.Default.Encode(string.IsNullOrEmpty(nextText) ? "›" : nextText));
        builder.Append("</button>");

        builder.Append("</div>");
        return builder.ToString();
    }
}
