namespace Kela.Core.Modules.Group.Requests;

public class AssignQuizzesRequest
{
    public List<long> QuizIds { get; set; } = new();
}
