using Kela.Web.Helpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Kela.Web.Shared.Buttons;

[HtmlTargetElement("app-btn")]
public sealed class BtnTagHelper : TagHelper
{
    public string? Variant { get; set; }
    public string? Icon { get; set; }
    public string? IconClass { get; set; }
    public bool Block { get; set; }
    public bool IconOnly { get; set; }
    public string? Href { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var hasHref = !string.IsNullOrWhiteSpace(Href);
        output.TagName = hasHref ? "a" : "button";
        output.TagMode = TagMode.StartTagAndEndTag;

        var cls = "btn";
        if (IconOnly)
        {
            cls += " btn-sm btn-square btn-ghost";
        }

        if (!string.IsNullOrWhiteSpace(Variant))
        {
            cls += " " + (Variant switch
            {
                "primary" => "btn-primary",
                "secondary" => "btn-secondary",
                "info" => "btn-info",
                "danger" => "btn-error",
                _ => $"btn-{Variant}",
            });
        }

        if (Block)
        {
            cls += " btn-block";
        }

        foreach (var attr in context.AllAttributes)
        {
            if (attr.Name == "class" && attr.Value is string userClass && !string.IsNullOrWhiteSpace(userClass))
            {
                cls += $" {userClass}";
            }
        }

        output.Attributes.SetAttribute("class", cls);

        if (hasHref)
        {
            output.Attributes.SetAttribute("href", Href);
        }
        else if (!context.AllAttributes.Any(a => a.Name == "type"))
        {
            output.Attributes.SetAttribute("type", "button");
        }

        var content = await output.GetChildContentAsync();
        var icon = string.IsNullOrWhiteSpace(Icon) ? "" : Icons.Icon(Icon, IconClass);
        output.Content.SetHtmlContent(icon + content.GetContent());
    }
}
