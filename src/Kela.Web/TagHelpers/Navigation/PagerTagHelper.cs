using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Kela.Web.TagHelpers.Navigation;

[HtmlTargetElement("app-pager")]
public sealed class PagerTagHelper : TagHelper
{
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public string? Url { get; set; }
    public string? HxTarget { get; set; }
    public string? HxSwap { get; set; }
    public string? HxInclude { get; set; }
    public string? PrevText { get; set; }
    public string? NextText { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = null;

        if (TotalPages <= 1)
        {
            output.SuppressOutput();
            return;
        }

        var baseUrl = string.IsNullOrWhiteSpace(Url) ? "" : Url;
        string UrlFor(int p) =>
            baseUrl + (baseUrl.Contains('?') ? "&" : "?") + $"page={p}";

        var hx = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(HxTarget))
        {
            hx.Append($" hx-target=\"{HtmlEncoder.Default.Encode(HxTarget)}\"");
        }
        if (!string.IsNullOrWhiteSpace(HxSwap))
        {
            hx.Append($" hx-swap=\"{HtmlEncoder.Default.Encode(HxSwap)}\"");
        }
        if (!string.IsNullOrWhiteSpace(HxInclude))
        {
            hx.Append($" hx-include=\"{HtmlEncoder.Default.Encode(HxInclude)}\"");
        }
        var hxAttrs = hx.ToString();

        var builder = new StringBuilder();
        builder.Append("<div class=\"pagination\">");

        builder.Append($"<button type=\"button\" class=\"pager-btn\"{(Page <= 1 ? " disabled" : "")} hx-get=\"{HtmlEncoder.Default.Encode(UrlFor(Page - 1))}\"{hxAttrs}>");
        builder.Append(HtmlEncoder.Default.Encode(string.IsNullOrEmpty(PrevText) ? "‹" : PrevText));
        builder.Append("</button>");

        for (var p = 1; p <= TotalPages; p++)
        {
            if (p == Page)
            {
                builder.Append($"<span class=\"pager-btn pager-current\">{p}</span>");
            }
            else if (p <= 2 || p >= TotalPages - 1 || Math.Abs(p - Page) <= 1)
            {
                builder.Append($"<button type=\"button\" class=\"pager-btn\" hx-get=\"{HtmlEncoder.Default.Encode(UrlFor(p))}\"{hxAttrs}>{p}</button>");
            }
            else if (p == 3 || p == TotalPages - 2)
            {
                builder.Append("<span class=\"pager-ellipsis\">…</span>");
            }
        }

        builder.Append($"<button type=\"button\" class=\"pager-btn\"{(Page >= TotalPages ? " disabled" : "")} hx-get=\"{HtmlEncoder.Default.Encode(UrlFor(Page + 1))}\"{hxAttrs}>");
        builder.Append(HtmlEncoder.Default.Encode(string.IsNullOrEmpty(NextText) ? "›" : NextText));
        builder.Append("</button>");

        builder.Append("</div>");
        output.Content.SetHtmlContent(builder.ToString());
    }
}
