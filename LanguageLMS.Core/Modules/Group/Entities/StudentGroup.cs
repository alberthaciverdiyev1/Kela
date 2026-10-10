using LanguageLMS.Core.Common.Entities;

namespace LanguageLMS.Core.Modules.Group.Entities;

using Student = LanguageLMS.Core.Modules.Auth.Entities.Student;

public class StudentGroup : BaseEntity
{
    public long StudentId { get; set; }

    public Student? Student { get; set; }

    public long GroupId { get; set; }

    public Group? Group { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }
}
