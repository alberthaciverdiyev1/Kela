using LanguageLMS.Core.Common.Entities;

namespace LanguageLMS.Core.Modules.Quiz.Entities;

public class StudentQuiz : BaseEntity
{
    public long StudentId { get; set; }

    public long QuizId { get; set; }

    public int TotalQuestionCount { get; set; }

    public int CompletedQuestionCount { get; set; }

    public bool IsCompleted { get; set; }

    public DateTimeOffset? StartDate { get; set; }

    public DateTimeOffset? EndDate { get; set; }

    public ICollection<StudentQuizQuestion> Answers { get; set; } = new List<StudentQuizQuestion>();
}
