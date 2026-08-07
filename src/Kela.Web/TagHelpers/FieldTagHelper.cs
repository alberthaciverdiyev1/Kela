using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Kela.Web.TagHelpers;

[HtmlTargetElement("app-field")]
public sealed class FieldTagHelper : TagHelper
{
    public string? For { get; set; }
    public string? Label { get; set; }
    public bool Required { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var content = await output.GetChildContentAsync();

        output.TagName = "div";
        output.Attributes.SetAttribute("class", "field");
        output.Content.AppendHtml("<label");
        if (!string.IsNullOrWhiteSpace(For))
        {
            output.Content.AppendHtml($" for=\"{HtmlEncoder.Default.Encode(For)}\"");
        }
        output.Content.AppendHtml(">");
        output.Content.Append(Label ?? "");
        if (Required)
        {
            output.Content.AppendHtml(" <span class=\"req\">*</span>");
        }
        output.Content.AppendHtml("</label>");
        output.Content.AppendHtml(content.GetContent());
    }
}
