using Kela.Domain.Attendances;
using Kela.Domain.Common;
using Kela.Domain.Subjects;
using Kela.Domain.Users;

namespace Kela.Domain.Lessons;

/// <summary>
/// Ders oturumu: bir öğretmenin, bir dersin, belirli bir zamandaki işlenişi.
/// Devamsızlık (Attendance) bu oturuma bağlıdır — böylece "kim, hangi ders, ne zaman" netleşir.
/// </summary>
public class Lesson : BaseEntity, ITenantEntity
{
    public int TenantId { get; set; }

    public int SubjectId { get; set; }
    public Subject? Subject { get; set; }

    /// <summary>FK → teachers.UserId (Teacher'in primary key'i).</summary>
    public int TeacherId { get; set; }
    public Teacher? Teacher { get; set; }

    public string Title { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public int DurationMinutes { get; set; } = 45;

    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}
