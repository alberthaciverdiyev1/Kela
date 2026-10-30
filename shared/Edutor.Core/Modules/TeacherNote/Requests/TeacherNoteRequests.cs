using Edutor.Core.Common.Requests;

namespace Edutor.Core.Modules.TeacherNote.Requests;

public class TeacherNoteListRequest : ListRequestBase
{
    public string? Search { get; set; }

    public bool? IsDone { get; set; }
}

public class TeacherNoteUpsertRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Body { get; set; }

    public DateOnly? Date { get; set; }

    public bool IsDone { get; set; }
}
