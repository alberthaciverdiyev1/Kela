using System.Text;
using System.Text.Encodings.Web;
using Kela.Web.Helpers;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Kela.Web.Shared.Overlay;

[HtmlTargetElement("app-modal")]
public sealed class ModalTagHelper(IAntiforgery antiforgery, IHttpContextAccessor http) : TagHelper
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? Icon { get; set; }
    public bool IconSuccess { get; set; }
    public bool Form { get; set; }
    public string? Action { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var content = await output.GetChildContentAsync();

        output.TagName = "dialog";
        output.Attributes.SetAttribute("class", "modal");
        if (!string.IsNullOrWhiteSpace(Id))
        {
            output.Attributes.SetAttribute("id", Id);
        }

        var builder = new StringBuilder();

        if (Form)
        {
            builder.Append("<form class=\"modal-card\" method=\"post\" novalidate");
            if (!string.IsNullOrWhiteSpace(Action))
            {
                builder.Append($" action=\"{HtmlEncoder.Default.Encode(Action)}\"");
            }
            builder.Append('>');

            var token = antiforgery.GetAndStoreTokens(http.HttpContext!).RequestToken ?? "";
            builder.Append($"<input name=\"__RequestVerificationToken\" type=\"hidden\" value=\"{HtmlEncoder.Default.Encode(token)}\" />");
        }
        else
        {
            builder.Append("<div class=\"modal-card\">");
        }

        builder.Append("<div class=\"modal-head\"><span class=\"modal-icon");
        if (IconSuccess)
        {
            builder.Append(" icon-success");
        }
        builder.Append("\">");
        builder.Append(Icons.Icon(Icon ?? ""));
        builder.Append("</span><h2>");
        builder.Append(HtmlEncoder.Default.Encode(Title ?? ""));
        builder.Append("</h2></div><div class=\"modal-body\">");
        builder.Append(content.GetContent());
        builder.Append("</div>");
        builder.Append(Form ? "</form>" : "</div>");

        output.Content.SetHtmlContent(builder.ToString());
    }
}
