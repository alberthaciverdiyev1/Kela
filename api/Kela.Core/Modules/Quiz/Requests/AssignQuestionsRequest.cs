namespace Kela.Core.Modules.Quiz.Requests;

public class AssignQuestionsRequest
{
    public List<long> QuestionIds { get; set; } = new();
}
