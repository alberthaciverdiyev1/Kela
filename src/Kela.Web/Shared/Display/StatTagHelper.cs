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
            "info" => ("bg-info/10", "text-info"),
            _ => ("bg-primary/10", "text-primary"),
        };

        var label = HtmlEncoder.Default.Encode(Label ?? "");
        var value = HtmlEncoder.Default.Encode(Value ?? "");

        output.TagName = null;
        output.Content.SetHtmlContent(
            $"<div class=\"flex items-center gap-4 rounded-box bg-base-100 p-5 shadow-sm\">" +
            $"<div class=\"flex size-12 items-center justify-center rounded-xl {iconBg} {iconColor}\">{Icons.Icon(Icon ?? "")}</div>" +
            $"<div><div class=\"text-sm text-base-content/60\">{label}</div>" +
            $"<div class=\"text-2xl font-bold\">{value}</div></div></div>");
    }
}
