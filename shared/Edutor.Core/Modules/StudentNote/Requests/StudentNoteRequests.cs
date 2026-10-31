namespace Edutor.Core.Modules.StudentNote.Requests;

public class StudentNoteListRequest
{
    public long? ContentId { get; set; }

    public long? LessonId { get; set; }

    /// <summary>"mine" | "shared" — boş olduqda hamısı.</summary>
    public string? Scope { get; set; }

    /// <summary>Yalnız materiala bağlanmayan müstəqil qeydlər.</summary>
    public bool? OnlyFree { get; set; }
}

public class StudentNoteUpsertRequest
{
    public long? ContentId { get; set; }

    public int? PositionSeconds { get; set; }

    public string? Title { get; set; }

    public string Body { get; set; } = string.Empty;

    public string Visibility { get; set; } = Entities.StudentNoteVisibility.Private;

    public long? GroupId { get; set; }
}
