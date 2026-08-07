using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Kela.Web.TagHelpers.Layout;

[HtmlTargetElement("app-card")]
public sealed class CardTagHelper : TagHelper
{
    public bool Center { get; set; }
    public bool Narrow { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var cls = "card";
        if (Center)
        {
            cls += " card-center";
        }
        if (Narrow)
        {
            cls += " card-narrow";
        }

        output.TagName = "div";
        output.Attributes.SetAttribute("class", cls);
    }
}
