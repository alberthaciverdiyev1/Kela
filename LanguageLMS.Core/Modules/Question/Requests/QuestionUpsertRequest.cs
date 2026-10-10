using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Core.Modules.Question.Requests;

public class QuestionUpsertRequest
{
    [FromForm(Name = "subject_id")]
    public long SubjectId { get; set; }

    public string? Title { get; set; }

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

    [FromForm(Name = "title_type")]
    public string TitleType { get; set; } = "text";

    [FromForm(Name = "variant_type")]
    public string VariantType { get; set; } = "text";

    // Şəkil rejimində yüklənən fayllar (verilərsə mətn əvəzinə istifadə olunur)
    [FromForm(Name = "title_image")]
    public IFormFile? TitleImage { get; set; }

    [FromForm(Name = "variant_a_image")]
    public IFormFile? VariantAImage { get; set; }

    [FromForm(Name = "variant_b_image")]
    public IFormFile? VariantBImage { get; set; }

    [FromForm(Name = "variant_c_image")]
    public IFormFile? VariantCImage { get; set; }

    [FromForm(Name = "variant_d_image")]
    public IFormFile? VariantDImage { get; set; }

    [FromForm(Name = "variant_e_image")]
    public IFormFile? VariantEImage { get; set; }
}
