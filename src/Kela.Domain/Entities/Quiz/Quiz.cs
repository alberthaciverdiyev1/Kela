namespace Kela.Domain.Entities;

public class Quiz
{
    public int ContentId { get; set; }
    public Content? Content { get; set; }

    public int TeacherId { get; set; }
    public User? Teacher { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();
}
