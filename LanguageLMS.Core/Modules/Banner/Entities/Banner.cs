using LanguageLMS.Core.Common.Entities;

namespace LanguageLMS.Core.Modules.Banner.Entities;

public class Banner : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    public string? Image { get; set; }
}
