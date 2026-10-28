using Edutor.Core.Common.Entities;

namespace Edutor.Core.Modules.Question.Entities;

public class Question : BaseEntity
{
    public long SubjectId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? VariantA { get; set; }
    public string? VariantB { get; set; }
    public string? VariantC { get; set; }
    public string? VariantD { get; set; }
    public string? VariantE { get; set; }

    /// <summary>Doğru variant: "A".."E".</summary>
    public string CorrectAnswer { get; set; } = "A";

    /// <summary>title mətn yoxsa şəkil: text | image</summary>
    public string TitleType { get; set; } = "text";

    /// <summary>variantlar mətn yoxsa şəkil: text | image</summary>
    public string VariantType { get; set; } = "text";

    public long? UserId { get; set; }
}
