using Kela.Core.Common.Entities;

namespace Kela.Core.Modules.Homework.Entities;

using Group = Kela.Core.Modules.Group.Entities.Group;

/// <summary>Tapşırığın hədəf qrupu. Yalnız qaralama mərhələsində dəyişdirilir.</summary>
public class HomeworkGroup : BaseEntity
{
    public long HomeworkId { get; set; }

    public Homework? Homework { get; set; }

    public long GroupId { get; set; }

    public Group? Group { get; set; }
}
