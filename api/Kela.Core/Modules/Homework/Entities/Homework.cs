using Kela.Core.Common.Entities;

namespace Kela.Core.Modules.Homework.Entities;

/// <summary>Qruplara verilən ev tapşırığı. Qaralama kimi yaranır, sonra yayımlanır.</summary>
public class Homework : BaseEntity
{
    /// <summary>Müəllim (admin yaradıbsa null).</summary>
    public long? UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsPublished { get; set; }

    public DateTimeOffset? PublishedAt { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<HomeworkGroup> Groups { get; set; } = new List<HomeworkGroup>();

    public ICollection<HomeworkQuiz> Quizzes { get; set; } = new List<HomeworkQuiz>();

    public ICollection<HomeworkAssignment> Assignments { get; set; } = new List<HomeworkAssignment>();
}
