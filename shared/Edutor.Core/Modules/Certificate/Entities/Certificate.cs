using Edutor.Core.Common.Entities;

namespace Edutor.Core.Modules.Certificate.Entities;

public class Certificate : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    public string TemplateKey { get; set; } = string.Empty;

    public string BodyText { get; set; } = string.Empty;

    public string? Signature { get; set; }
}
