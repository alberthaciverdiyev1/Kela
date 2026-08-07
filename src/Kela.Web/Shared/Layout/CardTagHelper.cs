using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Kela.Web.Shared.Layout;

[HtmlTargetElement("app-card")]
public sealed class CardTagHelper : TagHelper
{
    public bool Center { get; set; }
    public bool Narrow { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var cls = "card border border-base-200 bg-base-100 p-5";
        if (Center)
        {
            cls += " items-center text-center p-8";
        }
        if (Narrow)
        {
            cls += " w-full max-w-md mx-auto";
        }

        output.TagName = "div";
        output.Attributes.SetAttribute("class", cls);
    }
}
