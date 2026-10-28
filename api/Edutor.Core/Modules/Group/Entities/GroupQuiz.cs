using Edutor.Core.Common.Entities;

namespace Edutor.Core.Modules.Group.Entities;

using Quiz = Edutor.Core.Modules.Quiz.Entities.Quiz;

public class GroupQuiz : BaseEntity
{
    public long GroupId { get; set; }

    public Group? Group { get; set; }

    public long QuizId { get; set; }

    public Quiz? Quiz { get; set; }
}
