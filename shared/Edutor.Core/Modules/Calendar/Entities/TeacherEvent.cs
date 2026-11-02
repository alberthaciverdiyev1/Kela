using Edutor.Core.Common.Entities;

namespace Edutor.Core.Modules.Calendar.Entities;

/// <summary>Müəllimin öz təqvim hadisəsi (dərs, görüş, xatırlatma).</summary>
public class TeacherEvent : BaseEntity
{
    public long UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTimeOffset StartAt { get; set; }

    public DateTimeOffset EndAt { get; set; }

    public string Color { get; set; } = Common.Notes.NoteColors.Default;
}
