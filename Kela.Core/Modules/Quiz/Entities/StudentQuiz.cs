using Kela.Core.Common.Entities;

namespace Kela.Core.Modules.Quiz.Entities;

public class StudentQuiz : BaseEntity
{
    public long StudentId { get; set; }

    public long QuizId { get; set; }

    /// <summary>Neçənci cəhd (1, 2, 3...). is_repeat quizlərdə artır.</summary>
    public int AttemptTime { get; set; } = 1;

    public int TotalQuestionCount { get; set; }

    public int CompletedQuestionCount { get; set; }

    public bool IsCompleted { get; set; }

    public DateTimeOffset? StartDate { get; set; }

    public DateTimeOffset? EndDate { get; set; }

    public ICollection<StudentQuizQuestion> Answers { get; set; } = new List<StudentQuizQuestion>();
}
