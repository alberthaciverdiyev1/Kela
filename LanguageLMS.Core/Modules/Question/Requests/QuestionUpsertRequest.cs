using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Core.Modules.Question.Requests;

public class QuestionUpsertRequest
{
    [FromForm(Name = "subject_id")]
    public long SubjectId { get; set; }

    public string Title { get; set; } = string.Empty;

    [FromForm(Name = "variant_a")]
    public string? VariantA { get; set; }

    [FromForm(Name = "variant_b")]
    public string? VariantB { get; set; }

    [FromForm(Name = "variant_c")]
    public string? VariantC { get; set; }

    [FromForm(Name = "variant_d")]
    public string? VariantD { get; set; }

    [FromForm(Name = "variant_e")]
    public string? VariantE { get; set; }

    [FromForm(Name = "correct_answer")]
    public string CorrectAnswer { get; set; } = "A";

    [FromForm(Name = "variant_type")]
    public string VariantType { get; set; } = "text";
}
