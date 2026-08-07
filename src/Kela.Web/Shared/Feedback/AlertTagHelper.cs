using Kela.Web.Helpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Kela.Web.Shared.Feedback;

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

        var variant = type switch
        {
            "success" => "alert-success",
            "error" => "alert-error",
            "warning" => "alert-warning",
            _ => "alert-info",
        };

        var icon = string.IsNullOrWhiteSpace(Icon)
            ? type switch
            {
                "success" => "check",
                "error" => "x",
                "warning" => "clock",
                _ => "info",
            }
            : Icon;

        var content = await output.GetChildContentAsync();

        output.TagName = "div";
        output.Attributes.SetAttribute("class", $"alert {variant}");
        output.Content.AppendHtml(Icons.Icon(icon, "w-4 h-4"));
        output.Content.AppendHtml("<span>");
        output.Content.AppendHtml(content.GetContent());
        output.Content.AppendHtml("</span>");
    }
}
