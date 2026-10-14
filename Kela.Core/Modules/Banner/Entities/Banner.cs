using Kela.Core.Common.Entities;

namespace Kela.Core.Modules.Banner.Entities;

public class Banner : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    public string? Image { get; set; }
}
