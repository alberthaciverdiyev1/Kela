namespace Kela.Core.Modules.Quiz.Requests;

public class CompleteQuizRequest
{
    public long AttemptId { get; set; }

    public List<QuizAnswerRequest> Answers { get; set; } = new();
}

public class QuizAnswerRequest
{
    public long QuestionId { get; set; }

    public string? Answer { get; set; }
}
