using Edutor.Core.Common.Entities;

namespace Edutor.Core.Modules.Subject.Entities;

public class Subject : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public long? UserId { get; set; }

    /// <summary>Paylaşılan fənn: bütün müəllimlər fənnin özünü, dərslərini və materiallarını görür.</summary>
    public bool IsShared { get; set; }
}
