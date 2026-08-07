using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Kela.Web.TagHelpers.Display;

[HtmlTargetElement("app-avatar")]
public sealed class AvatarTagHelper : TagHelper
{
    public string? Initials { get; set; }
    public string? Size { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var cls = string.IsNullOrWhiteSpace(Size) ? "avatar" : $"avatar avatar-{Size}";
        var text = HtmlEncoder.Default.Encode(string.IsNullOrWhiteSpace(Initials) ? "?" : Initials);
        output.TagName = null;
        output.Content.SetHtmlContent($"<span class=\"{cls}\">{text}</span>");
    }
}
