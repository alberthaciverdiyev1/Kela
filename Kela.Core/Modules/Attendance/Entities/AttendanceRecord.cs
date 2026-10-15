using Kela.Core.Common.Entities;

namespace Kela.Core.Modules.Attendance.Entities;

using Group = Kela.Core.Modules.Group.Entities.Group;
using Student = Kela.Core.Modules.Auth.Entities.Student;

/// <summary>Bir şagirdin müəyyən gündə qrup dərsinə qatılıb-qatılmadığı.</summary>
public class AttendanceRecord : BaseEntity
{
    public long GroupId { get; set; }

    public Group? Group { get; set; }

    public long StudentId { get; set; }

    public Student? Student { get; set; }

    public DateOnly Date { get; set; }

    public bool IsPresent { get; set; }
}
