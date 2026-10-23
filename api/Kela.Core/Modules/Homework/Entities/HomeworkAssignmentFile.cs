using Kela.Core.Common.Entities;

namespace Kela.Core.Modules.Homework.Entities;

/// <summary>Tələbənin təqdim etdiyi fayl.</summary>
public class HomeworkAssignmentFile : BaseEntity
{
    public long AssignmentId { get; set; }

    public HomeworkAssignment? Assignment { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    public string? ContentType { get; set; }

    public long SizeBytes { get; set; }
}
