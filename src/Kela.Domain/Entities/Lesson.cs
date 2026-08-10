namespace Kela.Domain.Entities;

public class Lesson
{
    public int ContentId { get; set; }
    public Content? Content { get; set; }

    public int TeacherId { get; set; }
    public User? Teacher { get; set; }

    public string? VideoPath { get; set; }
    public string? ThumbnailPath { get; set; }
    public int DurationSeconds { get; set; }
    public bool IsPublished { get; set; }
    public int OrderIndex { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
