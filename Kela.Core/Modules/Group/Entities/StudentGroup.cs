using Kela.Core.Common.Entities;

namespace Kela.Core.Modules.Group.Entities;

using Student = Kela.Core.Modules.Auth.Entities.Student;

public class StudentGroup : BaseEntity
{
    public long StudentId { get; set; }

    public Student? Student { get; set; }

    public long GroupId { get; set; }

    public Group? Group { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }
}
