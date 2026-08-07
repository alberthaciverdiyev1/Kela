using System.Text.Encodings.Web;
using Kela.Web.Helpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Kela.Web.Shared.Display;

[HtmlTargetElement("app-stat")]
public sealed class StatTagHelper : TagHelper
{
    public string? Icon { get; set; }
    public string Tone { get; set; } = "primary";
    public string? Label { get; set; }
    public string? Value { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var (iconBg, iconColor) = Tone switch
        {
            "success" => ("bg-success/10", "text-success"),
            "warning" => ("bg-warning/10", "text-warning"),
            _ => ("bg-primary/10", "text-primary"),
        };

        var label = HtmlEncoder.Default.Encode(Label ?? "");
        var value = HtmlEncoder.Default.Encode(Value ?? "");

        output.TagName = null;
        output.Content.SetHtmlContent(
            $"<div class=\"stat flex items-center gap-3\"><div class=\"{iconBg} {iconColor} rounded-xl p-3 flex items-center\">" +
            $"{Icons.Icon(Icon ?? "")}</div><div><div class=\"stat-title\">{label}</div>" +
            $"<div class=\"stat-value\">{value}</div></div></div>");
    }
}
