using Kela.Domain.Common;

namespace Kela.Domain.Entities;

public class Attendance : BaseEntity
{
    public int WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;

    public int StudentId { get; set; }
    public User Student { get; set; } = null!;

    public DateOnly Date { get; set; }

    public AttendanceStatus Status { get; set; }

    public string? Note { get; set; }
}
