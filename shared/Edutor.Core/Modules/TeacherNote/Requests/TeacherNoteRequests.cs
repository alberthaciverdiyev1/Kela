using Edutor.Core.Common.Requests;
using Edutor.Core.Modules.TeacherNote.Entities;

namespace Edutor.Core.Modules.TeacherNote.Requests;

public class TeacherNoteListRequest : ListRequestBase
{
    public string? Search { get; set; }

    public bool? IsDone { get; set; }

    /// <summary>mine (yalnız öz) | shared (yalnız mənimlə paylaşılanlar).</summary>
    public string? Scope { get; set; }
}

public class TeacherNoteUpsertRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Body { get; set; }

    public DateOnly? Date { get; set; }

    public bool IsDone { get; set; }

    public string? Color { get; set; }

    public bool? IsPinned { get; set; }

    public long? ContentId { get; set; }

    public int? PositionSeconds { get; set; }

    public string Visibility { get; set; } = TeacherNoteVisibility.Private;

    public List<long> SharedWithUserIds { get; set; } = [];
}
