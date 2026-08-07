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
        var (from, to) = Tone switch
        {
            "success" => ("from-success", "to-success/70"),
            "info" => ("from-info", "to-info/70"),
            _ => ("from-primary", "to-primary/70"),
        };

        var initials = HtmlEncoder.Default.Encode(string.IsNullOrWhiteSpace(Initials) ? "?" : Initials);
        var title = HtmlEncoder.Default.Encode(Title ?? "");
        var chip = HtmlEncoder.Default.Encode(Chip ?? "");

        output.TagName = null;
        output.Content.SetHtmlContent(
            $"<div class=\"bg-linear-to-r {from} {to} text-white rounded-box shadow-lg p-6 flex items-center gap-4\">" +
            $"<div class=\"avatar placeholder\"><div class=\"bg-white/20 text-white rounded-full w-14 h-14 text-lg\"><span>{initials}</span></div></div>" +
            $"<div class=\"flex flex-col items-start gap-2\"><h1 class=\"text-xl font-bold\">{title}</h1>" +
            (string.IsNullOrWhiteSpace(Chip) ? "" : $"<span class=\"badge bg-white/20 text-white border-white/20\">{chip}</span>") +
            $"</div></div>");
    }
}
