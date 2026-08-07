using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Kela.Web.TagHelpers.Navigation;

[HtmlTargetElement("app-pager")]
public sealed class PagerTagHelper : TagHelper
{
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public string? PrevText { get; set; }
    public string? NextText { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = null;
        output.Content.SetHtmlContent(PagerHtml.Render(Page, TotalPages, PrevText, NextText));
    }
}
