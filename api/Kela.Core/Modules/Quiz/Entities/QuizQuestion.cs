using Kela.Core.Common.Entities;

namespace Kela.Core.Modules.Quiz.Entities;

public class QuizQuestion : BaseEntity
{
    public long QuizId { get; set; }

    public Quiz? Quiz { get; set; }

    public long QuestionId { get; set; }

    public int SortOrder { get; set; }
}
