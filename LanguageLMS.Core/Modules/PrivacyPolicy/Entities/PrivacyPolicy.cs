namespace LanguageLMS.Core.Modules.PrivacyPolicy.Entities;

public class PrivacyPolicy
{
    public long Id { get; set; }

    /// <summary>privacy | terms | about ...</summary>
    public string Type { get; set; } = "privacy";

    public string Content { get; set; } = string.Empty;
}
