using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Kela.Web.TagHelpers;

[HtmlTargetElement("app-panel-head")]
public sealed class PanelHeadTagHelper : TagHelper
{
    public string Tone { get; set; } = "primary";
    public string? Initials { get; set; }
    public string? Title { get; set; }
    public string? Chip { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var tone = Tone switch
        {
            "success" => "success",
            "info" => "info",
            _ => "primary",
        };

        var initials = HtmlEncoder.Default.Encode(string.IsNullOrWhiteSpace(Initials) ? "?" : Initials);
        var title = HtmlEncoder.Default.Encode(Title ?? "");
        var chip = HtmlEncoder.Default.Encode(Chip ?? "");

        output.TagName = null;
        output.Content.SetHtmlContent(
            $"<div class=\"panel-head panel-head-{tone}\"><span class=\"avatar avatar-lg\">{initials}</span>" +
            $"<div class=\"panel-head-text\"><h1>{title}</h1>" +
            (string.IsNullOrWhiteSpace(Chip) ? "" : $"<span class=\"chip\">{chip}</span>") +
            $"</div></div>");
    }
}
