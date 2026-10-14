using Kela.Core.Common.Entities;

namespace Kela.Core.Modules.Progress.Entities;

public class LessonProgress : BaseEntity
{
    public long StudentId { get; set; }

    public long LessonContentId { get; set; }

    public string Status { get; set; } = "completed";

    public DateTimeOffset? CompletedAt { get; set; }
}
