using System.Text.Encodings.Web;
using Kela.Web.Infrastructure;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Kela.Web.TagHelpers.Display;

[HtmlTargetElement("app-stat")]
public sealed class StatTagHelper : TagHelper
{
    public string? Icon { get; set; }
    public string Tone { get; set; } = "primary";
    public string? Label { get; set; }
    public string? Value { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var tone = Tone switch
        {
            "success" => "success",
            "warning" => "warning",
            _ => "primary",
        };

        var label = HtmlEncoder.Default.Encode(Label ?? "");
        var value = HtmlEncoder.Default.Encode(Value ?? "");

        output.TagName = null;
        output.Content.SetHtmlContent(
            $"<div class=\"stat-card\"><span class=\"stat-icon bg-{tone}-50 text-{tone}\">" +
            $"{Icons.Icon(Icon ?? "")}</span><div class=\"stat-meta\">" +
            $"<span class=\"stat-label\">{label}</span><span class=\"stat-value\">{value}</span>" +
            $"</div></div>");
    }
}
