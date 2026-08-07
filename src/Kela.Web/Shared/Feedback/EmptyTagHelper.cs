using System.Text.Encodings.Web;
using Kela.Web.Helpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Kela.Web.Shared.Feedback;

[HtmlTargetElement("app-empty")]
public sealed class EmptyTagHelper : TagHelper
{
    public string? Icon { get; set; }
    public string? Text { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var text = HtmlEncoder.Default.Encode(Text ?? "");

        output.TagName = null;
        output.Content.SetHtmlContent(
            $"<div class=\"card bg-base-100 shadow items-center text-center py-10 px-4\">" +
            $"<span class=\"text-base-300 mb-3\">{Icons.Icon(Icon ?? "students", "w-12 h-12")}</span>" +
            (Text is null ? "" : $"<p class=\"text-base-content/60 max-w-sm\">{text}</p>") +
            $"</div>");
    }
}
