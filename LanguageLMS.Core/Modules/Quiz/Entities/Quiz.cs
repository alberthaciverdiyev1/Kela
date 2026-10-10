using LanguageLMS.Core.Common.Entities;
using LanguageLMS.Core.Common.Sorting;

namespace LanguageLMS.Core.Modules.Quiz.Entities;

public class Quiz : BaseEntity, ISortable
{
    public long? LessonId { get; set; }

    public string Title { get; set; } = string.Empty;

    public int DurationMinutes { get; set; }

    /// <summary>"lesson" (dərs quizi) veya "general" (ümumi).</summary>
    public string Type { get; set; } = "lesson";

    public bool IsActive { get; set; } = true;

    public bool IsRepeat { get; set; }

    public int SortOrder { get; set; }

    public long? UserId { get; set; }

    public ICollection<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();
}
