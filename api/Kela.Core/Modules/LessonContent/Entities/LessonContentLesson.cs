using Kela.Core.Common.Entities;

namespace Kela.Core.Modules.LessonContent.Entities;

using Lesson = Kela.Core.Modules.Lesson.Entities.Lesson;

public class LessonContentLesson : BaseEntity
{
    public long LessonContentId { get; set; }

    public LessonContent? Content { get; set; }

    public long LessonId { get; set; }

    public Lesson? Lesson { get; set; }
}
