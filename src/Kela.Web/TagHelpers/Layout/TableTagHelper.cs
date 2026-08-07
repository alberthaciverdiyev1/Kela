using System.Text;
using System.Text.Encodings.Web;
using Kela.Web.Infrastructure;
using Kela.Web.Localization;
using Kela.Web.TagHelpers.Navigation;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Kela.Web.TagHelpers.Layout;

[HtmlTargetElement("app-table")]
public sealed class TableTagHelper(Localizer localizer) : TagHelper
{
    public string[]? Columns { get; set; }
    public bool ActionsColumn { get; set; }
    public bool Empty { get; set; }
    public string? EmptyIcon { get; set; }
    public string? EmptyText { get; set; }
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public string? PrevText { get; set; }
    public string? NextText { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = null;

        if (Empty)
        {
            var text = HtmlEncoder.Default.Encode(EmptyText ?? "");
            output.Content.SetHtmlContent(
                $"<div class=\"card card-center\"><span class=\"empty-icon\">{Icons.Icon(EmptyIcon ?? "students", "icon-xl")}</span>" +
                (string.IsNullOrWhiteSpace(EmptyText) ? "" : $"<p class=\"empty-text\">{text}</p>") +
                $"</div>");
            return;
        }

        var content = await output.GetChildContentAsync();

        var builder = new StringBuilder();
        builder.Append("<div class=\"card table-card\"><div class=\"table-scroll\"><table class=\"data-table\">");
        builder.Append("<thead><tr>");
        if (Columns is not null)
        {
            foreach (var key in Columns)
            {
                builder.Append($"<th>{HtmlEncoder.Default.Encode(localizer.T(key))}</th>");
            }
        }
        if (ActionsColumn)
        {
            builder.Append("<th class=\"col-actions\"></th>");
        }
        builder.Append("</tr></thead>");
        builder.Append(content.GetContent());
        builder.Append("</table></div>");
        if (TotalPages > 1)
        {
            builder.Append(PagerHtml.Render(Page, TotalPages, PrevText, NextText));
        }
        builder.Append("</div>");

        output.Content.SetHtmlContent(builder.ToString());
    }
}
