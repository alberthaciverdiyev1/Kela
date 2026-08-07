using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Kela.Web.Shared.Layout;

[HtmlTargetElement("app-panel-head")]
public sealed class PanelHeadTagHelper : TagHelper
{
    public string? Initials { get; set; }
    public string? Title { get; set; }
    public string? Chip { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var initials = HtmlEncoder.Default.Encode(string.IsNullOrWhiteSpace(Initials) ? "?" : Initials);
        var title = HtmlEncoder.Default.Encode(Title ?? "");
        var chip = HtmlEncoder.Default.Encode(Chip ?? "");

        output.TagName = null;
        output.Content.SetHtmlContent(
            $"<div class=\"flex items-center gap-3 border-b border-base-200 pb-4\">" +
            $"<div class=\"flex size-11 items-center justify-center rounded-full border border-base-300 bg-base-100 text-sm font-semibold text-base-content/70\">{initials}</div>" +
            $"<div class=\"flex flex-col items-start gap-1.5\"><h1 class=\"text-xl font-semibold tracking-tight text-base-content\">{title}</h1>" +
            (string.IsNullOrWhiteSpace(Chip) ? "" : $"<span class=\"badge badge-ghost badge-sm\">{chip}</span>") +
            $"</div></div>");
    }
}
