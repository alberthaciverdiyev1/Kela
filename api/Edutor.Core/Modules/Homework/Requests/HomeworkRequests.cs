using Edutor.Core.Common.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Edutor.Core.Modules.Homework.Requests;

public class HomeworkListRequest : ListRequestBase
{
    public string? Search { get; set; }

    [FromQuery(Name = "group_id")]
    public long? GroupId { get; set; }

    /// <summary>null = hamısı, true = yalnız yayımlanmış, false = yalnız qaralama.</summary>
    [FromQuery(Name = "is_published")]
    public bool? IsPublished { get; set; }

    [FromQuery(Name = "is_deleted")]
    public int IsDeleted { get; set; }
}

public class HomeworkUpsertRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [FromForm(Name = "group_ids")]
    public List<long> GroupIds { get; set; } = new();

    /// <summary>GroupIds ilə indeks üzrə uyğun gələn son tarixlər.</summary>
    [FromForm(Name = "group_due_dates")]
    public List<DateTimeOffset> GroupDueDates { get; set; } = new();

    /// <summary>GroupIds ilə indeks üzrə uyğun gələn maksimum ballar.</summary>
    [FromForm(Name = "group_scores")]
    public List<decimal> GroupScores { get; set; } = new();

    [FromForm(Name = "quiz_ids")]
    public List<long> QuizIds { get; set; } = new();

    /// <summary>Yalnız admin başqa müəllim adına yaradanda istifadə olunur.</summary>
    [FromForm(Name = "user_id")]
    public long? UserId { get; set; }
}

public class HomeworkAssignmentListRequest : ListRequestBase
{
    public string? Search { get; set; }

    public string? Status { get; set; }
}

public class GradeHomeworkRequest
{
    public decimal Score { get; set; }

    public string? Feedback { get; set; }

    [FromForm(Name = "return_to_student")]
    public bool ReturnToStudent { get; set; }
}

public class SubmitHomeworkRequest
{
    public string? Text { get; set; }

    public string? Link { get; set; }

    [FromForm(Name = "files")]
    public List<IFormFile> Files { get; set; } = new();
}
