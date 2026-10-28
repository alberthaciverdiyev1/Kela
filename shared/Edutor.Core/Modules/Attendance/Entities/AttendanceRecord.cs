using Edutor.Core.Common.Entities;

namespace Edutor.Core.Modules.Attendance.Entities;

using Group = Edutor.Core.Modules.Group.Entities.Group;
using Student = Edutor.Core.Modules.Auth.Entities.Student;

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
