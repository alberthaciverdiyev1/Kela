using Edutor.Core.Common.Entities;

namespace Edutor.Core.Modules.StudentNote.Entities;

/// <summary>
/// Şagirdin şəxsi qeydi. Materiala (video/səs) bağlana bilər və ya müstəqil olur.
/// Görünürlük şagirdin seçiminə görə yalnız özünə və ya seçilmiş qrupun üzvlərinə açıqdır.
/// </summary>
public class StudentNote : BaseEntity
{
    public long StudentId { get; set; }

    public long? LessonId { get; set; }

    public long? LessonContentId { get; set; }

    /// <summary>Media daxilindəki an (saniyə). Yalnız video/səs qeydlərində olur.</summary>
    public int? PositionSeconds { get; set; }

    public string? Title { get; set; }

    public string Body { get; set; } = string.Empty;

    public string Visibility { get; set; } = StudentNoteVisibility.Private;

    /// <summary>Paylaşılan qeyd üçün həmin qrupun üzvləri qeydi görür.</summary>
    public long? GroupId { get; set; }
}

public static class StudentNoteVisibility
{
    public const string Private = "private";
    public const string Group = "group";
}
