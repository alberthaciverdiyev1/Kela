namespace Edutor.Core.Modules.Calendar.Requests;

public class TeacherEventUpsertRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTimeOffset StartAt { get; set; }

    public DateTimeOffset EndAt { get; set; }

    public string? Color { get; set; }
}
