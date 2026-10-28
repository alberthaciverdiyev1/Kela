using Edutor.Core.Common.Entities;

namespace Edutor.Core.Modules.Homework.Entities;

using Quiz = Edutor.Core.Modules.Quiz.Entities.Quiz;

/// <summary>Tapşırığa əlavə olunan mövcud quiz. Nəticə mövcud quiz cəhdlərindən oxunur.</summary>
public class HomeworkQuiz : BaseEntity
{
    public long HomeworkId { get; set; }

    public Homework? Homework { get; set; }

    public long QuizId { get; set; }

    public Quiz? Quiz { get; set; }

    public int SortOrder { get; set; }
}
