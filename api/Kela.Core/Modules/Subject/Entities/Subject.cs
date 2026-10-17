using Kela.Core.Common.Entities;

namespace Kela.Core.Modules.Subject.Entities;

public class Subject : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public long? UserId { get; set; }
}
