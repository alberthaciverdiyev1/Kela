using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Kela.Web.Shared.Layout;

[HtmlTargetElement("app-page-head")]
public sealed class PageHeadTagHelper : TagHelper
{
    public string? Title { get; set; }
    public string? Subtitle { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var content = await output.GetChildContentAsync();

        output.TagName = "div";
        output.Attributes.SetAttribute("class", "flex items-start justify-between gap-4 flex-wrap");
        output.Content.AppendHtml("<div><h1 class=\"text-2xl font-bold text-base-content\">");
        output.Content.Append(Title ?? "");
        output.Content.AppendHtml("</h1>");
        if (Subtitle is not null)
        {
            output.Content.AppendHtml("<p class=\"text-sm text-base-content/60\">");
            output.Content.Append(Subtitle);
            output.Content.AppendHtml("</p>");
        }
        output.Content.AppendHtml("</div>");
        output.Content.AppendHtml(content.GetContent());
    }
}
