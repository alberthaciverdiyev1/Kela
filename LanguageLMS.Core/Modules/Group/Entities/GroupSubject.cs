using LanguageLMS.Core.Common.Entities;

namespace LanguageLMS.Core.Modules.Group.Entities;

using Subject = LanguageLMS.Core.Modules.Subject.Entities.Subject;

public class GroupSubject : BaseEntity
{
    public long GroupId { get; set; }

    public Group? Group { get; set; }

    public long SubjectId { get; set; }

    public Subject? Subject { get; set; }
}
