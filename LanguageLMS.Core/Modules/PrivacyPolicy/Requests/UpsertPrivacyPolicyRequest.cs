using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Core.Modules.PrivacyPolicy.Requests;

public class UpsertPrivacyPolicyRequest
{
    [FromForm(Name = "type")]
    public string Type { get; set; } = "privacy";

    public string Content { get; set; } = string.Empty;
}
