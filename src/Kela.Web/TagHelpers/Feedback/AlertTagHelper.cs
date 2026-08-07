using Kela.Web.Infrastructure;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Kela.Web.TagHelpers.Feedback;

[HtmlTargetElement("app-alert")]
public sealed class AlertTagHelper : TagHelper
{
    public string Type { get; set; } = "info";
    public string? Icon { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var type = Type switch
        {
            "success" => "success",
            "error" => "error",
            "warning" => "warning",
            _ => "info",
        };

        var icon = string.IsNullOrWhiteSpace(Icon)
            ? type switch
            {
                "success" => "check",
                "error" => "lock",
                "warning" => "lock",
                _ => "sparkles",
            }
            : Icon;

        var content = await output.GetChildContentAsync();

        output.TagName = "div";
        output.Attributes.SetAttribute("class", $"alert alert-{type}");
        output.Content.AppendHtml(Icons.Icon(icon, "icon-sm"));
        output.Content.AppendHtml("<span>");
        output.Content.AppendHtml(content.GetContent());
        output.Content.AppendHtml("</span>");
    }
}
