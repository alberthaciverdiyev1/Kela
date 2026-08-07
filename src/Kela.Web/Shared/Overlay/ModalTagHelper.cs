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
            builder.Append("<form class=\"modal-box p-0 overflow-hidden\" method=\"post\" novalidate");
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
            builder.Append("<div class=\"modal-box p-0 overflow-hidden\">");
        }

        builder.Append("<div class=\"flex items-center gap-3 border-b border-base-200 px-6 py-4\">");
        builder.Append("<span class=\"flex size-9 items-center justify-center rounded-lg ");
        builder.Append(IconSuccess ? "bg-success/10 text-success" : "bg-primary/10 text-primary");
        builder.Append("\">");
        builder.Append(Icons.Icon(Icon ?? ""));
        builder.Append("</span><h2 class=\"text-lg font-bold\">");
        builder.Append(HtmlEncoder.Default.Encode(Title ?? ""));
        builder.Append("</h2></div><div class=\"flex flex-col gap-4 px-6 py-5\">");
        builder.Append(content.GetContent());
        builder.Append("</div>");
        builder.Append(Form ? "</form>" : "</div>");

        output.Content.SetHtmlContent(builder.ToString());
    }
}
