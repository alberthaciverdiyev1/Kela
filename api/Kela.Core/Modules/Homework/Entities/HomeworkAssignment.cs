using Kela.Core.Common.Entities;

namespace Kela.Core.Modules.Homework.Entities;

using Student = Kela.Core.Modules.Auth.Entities.Student;

/// <summary>Yayım anında hər tələbə üçün yaranan snapshot — girişin yeganə mənbəyi.</summary>
public class HomeworkAssignment : BaseEntity
{
    public long HomeworkId { get; set; }

    public Homework? Homework { get; set; }

    public long StudentId { get; set; }

    public Student? Student { get; set; }

    /// <summary>Snapshot götürülərkən mənbə qrup.</summary>
    public long? GroupId { get; set; }

    /// <summary>Yayım anındaki qrup son tarixi — sonradan dəyişmir.</summary>
    public DateTimeOffset DueDate { get; set; }

    public decimal MaxScore { get; set; } = 100;

    public string Status { get; set; } = HomeworkStatus.Assigned;

    public string? Text { get; set; }

    public string? Link { get; set; }

    public DateTimeOffset? SubmittedAt { get; set; }

    public bool IsLate { get; set; }

    public int AttemptCount { get; set; }

    public decimal? Score { get; set; }

    public string? Feedback { get; set; }

    public DateTimeOffset? GradedAt { get; set; }

    public long? GradedByUserId { get; set; }

    public DateTimeOffset? ReturnedAt { get; set; }

    public ICollection<HomeworkAssignmentFile> Files { get; set; } = new List<HomeworkAssignmentFile>();
}

public static class HomeworkStatus
{
    public const string Assigned = "assigned";
    public const string Submitted = "submitted";
    public const string Graded = "graded";
    public const string Returned = "returned";
}
