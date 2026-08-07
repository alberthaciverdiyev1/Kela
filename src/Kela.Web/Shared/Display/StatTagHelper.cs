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
        var iconColor = Tone switch
        {
            "success" => "text-success",
            "warning" => "text-warning",
            "info" => "text-info",
            _ => "text-primary",
        };

        var label = HtmlEncoder.Default.Encode(Label ?? "");
        var value = HtmlEncoder.Default.Encode(Value ?? "");

        output.TagName = null;
        output.Content.SetHtmlContent(
            $"<div class=\"flex items-center gap-4 rounded-box border border-base-200 bg-base-100 px-5 py-4\">" +
            $"<span class=\"{iconColor}\">{Icons.Icon(Icon ?? "", "w-4 h-4")}</span>" +
            $"<div><div class=\"text-xs font-medium uppercase tracking-wide text-base-content/50\">{label}</div>" +
            $"<div class=\"text-2xl font-semibold tracking-tight text-base-content\">{value}</div></div></div>");
    }
}
