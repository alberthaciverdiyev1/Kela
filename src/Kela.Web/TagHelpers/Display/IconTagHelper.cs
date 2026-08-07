using Kela.Web.Infrastructure;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Kela.Web.TagHelpers.Display;

[HtmlTargetElement("app-icon")]
public sealed class IconTagHelper : TagHelper
{
    public string Name { get; set; } = "";
    public string? Class { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = null;
        output.Content.SetHtmlContent(Icons.Icon(Name, Class));
    }
}
