using LanguageLMS.Core.Common.Entities;

namespace LanguageLMS.Core.Modules.MainCategory.Entities;

public class MainCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public long? UserId { get; set; }
}
