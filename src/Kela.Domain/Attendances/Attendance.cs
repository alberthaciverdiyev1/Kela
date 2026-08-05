using Kela.Domain.Attendances.Enums;
using Kela.Domain.Common;
using Kela.Domain.Subjects;
using Kela.Domain.Users;

namespace Kela.Domain.Attendances;

public class Attendance : BaseEntity, ITenantEntity
{
    public int TenantId { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }

    public int SubjectId { get; set; }
    public Subject? Subject { get; set; }

    public AttendanceStatus Status { get; set; }
}
