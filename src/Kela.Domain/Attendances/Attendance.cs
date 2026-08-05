using Kela.Domain.Attendances.Enums;
using Kela.Domain.Common;
using Kela.Domain.Lessons;
using Kela.Domain.Users;

namespace Kela.Domain.Attendances;


public class Attendance : BaseEntity, ITenantEntity
{
    public int TenantId { get; set; }
    public int LessonId { get; set; }
    public Lesson? Lesson { get; set; }
    public int StudentId { get; set; }
    public Student? Student { get; set; }
    public AttendanceStatus Status { get; set; }
}
