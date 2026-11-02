using Edutor.Core.Common.Entities;

namespace Edutor.Core.Modules.TeacherNote.Entities;

public class TeacherNote : BaseEntity
{
    public long UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Body { get; set; }

    public DateOnly? Date { get; set; }

    public bool IsDone { get; set; }

    public string Color { get; set; } = Common.Notes.NoteColors.Default;

    public bool IsPinned { get; set; }

    public long? ContentId { get; set; }

    public int? PositionSeconds { get; set; }

    public string Visibility { get; set; } = TeacherNoteVisibility.Private;

    public ICollection<TeacherNoteShare> Shares { get; set; } = new List<TeacherNoteShare>();
}

public static class TeacherNoteVisibility
{
    public const string Private = "private";
    public const string Shared = "shared";
}
