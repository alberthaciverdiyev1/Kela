using System.Text;
using System.Text.Encodings.Web;
using Kela.Web.Infrastructure;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Kela.Web.TagHelpers;

[HtmlTargetElement("app-modal")]
public sealed class ModalTagHelper(IAntiforgery antiforgery, IHttpContextAccessor http) : TagHelper
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? Icon { get; set; }
    public bool IconSuccess { get; set; }
    public bool Form { get; set; }
    public string? HxPost { get; set; }
    public string? HxTarget { get; set; }
    public string? HxSwap { get; set; }
    public string? HxDisabledElt { get; set; }
    public string? HxSwapOob { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var content = await output.GetChildContentAsync();

        output.TagName = "dialog";
        output.Attributes.SetAttribute("class", "modal");
        if (!string.IsNullOrWhiteSpace(Id))
        {
            output.Attributes.SetAttribute("id", Id);
        }
        if (!string.IsNullOrWhiteSpace(HxSwapOob))
        {
            output.Attributes.SetAttribute("hx-swap-oob", HxSwapOob);
        }

        var builder = new StringBuilder();

        if (Form)
        {
            builder.Append("<form class=\"modal-card\" novalidate");
            if (!string.IsNullOrWhiteSpace(HxPost))
            {
                builder.Append($" hx-post=\"{HtmlEncoder.Default.Encode(HxPost)}\"");
            }
            if (!string.IsNullOrWhiteSpace(HxTarget))
            {
                builder.Append($" hx-target=\"{HtmlEncoder.Default.Encode(HxTarget)}\"");
            }
            if (!string.IsNullOrWhiteSpace(HxSwap))
            {
                builder.Append($" hx-swap=\"{HtmlEncoder.Default.Encode(HxSwap)}\"");
            }
            if (!string.IsNullOrWhiteSpace(HxDisabledElt))
            {
                builder.Append($" hx-disabled-elt=\"{HtmlEncoder.Default.Encode(HxDisabledElt)}\"");
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
