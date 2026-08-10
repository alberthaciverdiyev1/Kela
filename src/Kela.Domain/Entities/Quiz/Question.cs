using Kela.Domain.Common;

namespace Kela.Domain.Entities;

public class Question : BaseEntity
{
    public int TeacherId { get; set; }
    public User? Teacher { get; set; }

    public string Text { get; set; } = string.Empty;
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string? OptionD { get; set; }
    public string? OptionE { get; set; }
    public int CorrectOption { get; set; }

    public ICollection<QuizQuestion> Quizzes { get; set; } = new List<QuizQuestion>();
}
