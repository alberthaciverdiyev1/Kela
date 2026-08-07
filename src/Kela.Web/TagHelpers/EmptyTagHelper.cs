using System.Text.Encodings.Web;
using Kela.Web.Infrastructure;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Kela.Web.TagHelpers;

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
            $"<div class=\"card card-center\"><span class=\"empty-icon\">{Icons.Icon(Icon ?? "students", "icon-xl")}</span>" +
            (Text is null ? "" : $"<p class=\"empty-text\">{text}</p>") +
            $"</div>");
    }
}
