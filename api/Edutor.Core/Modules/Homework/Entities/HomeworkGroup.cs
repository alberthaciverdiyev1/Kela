using Edutor.Core.Common.Entities;

namespace Edutor.Core.Modules.Homework.Entities;

using Group = Edutor.Core.Modules.Group.Entities.Group;

/// <summary>Tapşırığın hədəf qrupu. Yalnız qaralama mərhələsində dəyişdirilir.</summary>
public class HomeworkGroup : BaseEntity
{
    public long HomeworkId { get; set; }

    public Homework? Homework { get; set; }

    public long GroupId { get; set; }

    public Group? Group { get; set; }

    public DateTimeOffset DueDate { get; set; }

    public decimal MaxScore { get; set; } = 100;
}
