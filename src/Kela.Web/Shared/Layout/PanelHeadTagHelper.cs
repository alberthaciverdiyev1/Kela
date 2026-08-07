using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Kela.Web.Shared.Layout;

[HtmlTargetElement("app-panel-head")]
public sealed class PanelHeadTagHelper : TagHelper
{
    public string Tone { get; set; } = "primary";
    public string? Initials { get; set; }
    public string? Title { get; set; }
    public string? Chip { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var badge = Tone switch
        {
            "success" => "badge-success",
            "info" => "badge-info",
            "warning" => "badge-warning",
            _ => "badge-primary",
        };

        var initials = HtmlEncoder.Default.Encode(string.IsNullOrWhiteSpace(Initials) ? "?" : Initials);
        var title = HtmlEncoder.Default.Encode(Title ?? "");
        var chip = HtmlEncoder.Default.Encode(Chip ?? "");

        output.TagName = null;
        output.Content.SetHtmlContent(
            $"<div class=\"flex items-center gap-4 rounded-box bg-base-100 p-5 shadow-sm\">" +
            $"<div class=\"avatar placeholder\"><div class=\"bg-primary text-white rounded-full w-14 h-14 text-lg\"><span>{initials}</span></div></div>" +
            $"<div class=\"flex flex-col items-start gap-1.5\"><h1 class=\"text-2xl font-bold\">{title}</h1>" +
            (string.IsNullOrWhiteSpace(Chip) ? "" : $"<span class=\"badge {badge}\">{chip}</span>") +
            $"</div></div>");
    }
}
