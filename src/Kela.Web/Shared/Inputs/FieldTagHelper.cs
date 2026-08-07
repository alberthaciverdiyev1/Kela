using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Kela.Web.Shared.Inputs;

[HtmlTargetElement("app-field")]
public sealed class FieldTagHelper : TagHelper
{
    [ViewContext]
    public ViewContext ViewContext { get; set; } = default!;

    public ModelExpression? For { get; set; }
    public string? Label { get; set; }
    public bool Required { get; set; }
    public string? Type { get; set; }
    public string? Autocomplete { get; set; }
    public string? InputMode { get; set; }
    public string? Placeholder { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var child = await output.GetChildContentAsync();
        var hasChild = !string.IsNullOrWhiteSpace(child.GetContent());

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", "field");

        output.Content.AppendHtml("<label");
        if (For is not null)
        {
            output.Content.AppendHtml($" for=\"{HtmlEncoder.Default.Encode(For.Name)}\"");
        }
        output.Content.AppendHtml(">");
        output.Content.Append(Label ?? "");
        if (Required)
        {
            output.Content.AppendHtml(" <span class=\"req\">*</span>");
        }
        output.Content.AppendHtml("</label>");

        if (hasChild)
        {
            output.Content.AppendHtml(child.GetContent());
        }
        else if (For is not null)
        {
            output.Content.AppendHtml(BuildInput());
            output.Content.AppendHtml(BuildError());
        }
    }

    private string BuildInput()
    {
        var name = For!.Name;
        var type = Type ?? InferType(name);
        var autocomplete = Autocomplete ?? InferAutocomplete(name);
        var value = HtmlEncoder.Default.Encode(For.Model?.ToString() ?? "");

        var sb = new StringBuilder();
        sb.Append($"<input id=\"{HtmlEncoder.Default.Encode(name)}\" name=\"{HtmlEncoder.Default.Encode(name)}\" type=\"{type}\" class=\"input\" value=\"{value}\"");
        if (!string.IsNullOrWhiteSpace(autocomplete))
        {
            sb.Append($" autocomplete=\"{autocomplete}\"");
        }
        if (!string.IsNullOrWhiteSpace(InputMode))
        {
            sb.Append($" inputmode=\"{InputMode}\"");
        }
        if (!string.IsNullOrWhiteSpace(Placeholder))
        {
            sb.Append($" placeholder=\"{HtmlEncoder.Default.Encode(Placeholder)}\"");
        }
        sb.Append("/>");
        return sb.ToString();
    }

    private string BuildError()
    {
        var name = For!.Name;
        var error = ViewContext.ModelState.TryGetValue(name, out var entry)
            ? entry.Errors.FirstOrDefault(e => !string.IsNullOrWhiteSpace(e.ErrorMessage))?.ErrorMessage
            : null;
        return string.IsNullOrWhiteSpace(error)
            ? ""
            : $"<span class=\"field-error\">{HtmlEncoder.Default.Encode(error)}</span>";
    }

    private static string InferType(string name)
    {
        if (name.Contains("email", StringComparison.OrdinalIgnoreCase)) return "email";
        if (name.Contains("password", StringComparison.OrdinalIgnoreCase)) return "password";
        if (name.Contains("phone", StringComparison.OrdinalIgnoreCase)) return "tel";
        return "text";
    }

    private static string InferAutocomplete(string name)
    {
        if (name.Contains("email", StringComparison.OrdinalIgnoreCase)) return "email";
        if (name.Contains("password", StringComparison.OrdinalIgnoreCase)) return "current-password";
        if (name.Contains("phone", StringComparison.OrdinalIgnoreCase)) return "tel";
        return "off";
    }
}
