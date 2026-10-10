using LanguageLMS.Core.Common.Entities;

namespace LanguageLMS.Core.Modules.Quiz.Entities;

public class StudentQuizQuestion : BaseEntity
{
    public long StudentQuizId { get; set; }

    public StudentQuiz? StudentQuiz { get; set; }

    public long QuestionId { get; set; }

    public string? StudentAnswer { get; set; }

    public bool IsRight { get; set; }
}
