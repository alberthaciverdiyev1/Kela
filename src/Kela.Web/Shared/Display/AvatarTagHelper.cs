using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Kela.Web.Shared.Display;

[HtmlTargetElement("app-avatar")]
public sealed class AvatarTagHelper : TagHelper
{
    public string? Initials { get; set; }
    public string? Size { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var sizeCls = string.IsNullOrWhiteSpace(Size) ? "w-8 h-8 text-xs" : "w-14 h-14 text-lg";
        var text = HtmlEncoder.Default.Encode(string.IsNullOrWhiteSpace(Initials) ? "?" : Initials);
        output.TagName = null;
        output.Content.SetHtmlContent(
            $"<div class=\"avatar placeholder\"><div class=\"bg-primary text-white rounded-full {sizeCls}\">" +
            $"<span>{text}</span></div></div>");
    }
}
