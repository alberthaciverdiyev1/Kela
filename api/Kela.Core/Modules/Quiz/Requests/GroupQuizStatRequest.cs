using Microsoft.AspNetCore.Mvc;

namespace Kela.Core.Modules.Quiz.Requests;

public class GroupQuizStatRequest
{
    /// <summary>lesson | general | (boş = hamısı)</summary>
    [FromQuery(Name = "quiz_type")]
    public string? QuizType { get; set; }
}
