using Edutor.Core.Common.Entities;

namespace Edutor.Core.Modules.LessonContent.Entities;

using Lesson = Edutor.Core.Modules.Lesson.Entities.Lesson;

public class LessonContentLesson : BaseEntity
{
    public long LessonContentId { get; set; }

    public LessonContent? Content { get; set; }

    public long LessonId { get; set; }

    public Lesson? Lesson { get; set; }

    /// <summary>Bu dərs daxilində materialın sırası.</summary>
    public int SortOrder { get; set; }
}
