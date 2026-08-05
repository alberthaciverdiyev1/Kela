using Kela.Domain.Common;
using Kela.Domain.Users;
using Kela.Domain.Users.Enums;

namespace Kela.Domain;

public class Attendance : BaseEntity
{
    public int UserId { get; set; }
    public User? User { get; set; }

    public int SubjectId { get; set; }
    public Subject? Subject { get; set; }

    public AttendanceStatus Status { get; set; }
}
