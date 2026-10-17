using Kela.Core.Common.Entities;

namespace Kela.Core.Modules.Group.Entities;

using Quiz = Kela.Core.Modules.Quiz.Entities.Quiz;

public class GroupQuiz : BaseEntity
{
    public long GroupId { get; set; }

    public Group? Group { get; set; }

    public long QuizId { get; set; }

    public Quiz? Quiz { get; set; }
}
