using Kela.Core.Common.Entities;

namespace Kela.Core.Modules.Group.Entities;

public class MainCategoryAssignmentLog : BaseEntity
{
    public long SubjectId { get; set; }

    public long GroupId { get; set; }

    /// <summary>attach | detach</summary>
    public string Action { get; set; } = string.Empty;
}
