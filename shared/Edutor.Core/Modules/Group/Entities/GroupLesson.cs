using Edutor.Core.Common.Entities;

namespace Edutor.Core.Modules.Group.Entities;

using Lesson = Edutor.Core.Modules.Lesson.Entities.Lesson;

/// <summary>Qrupa fənnlə birlikdə açılan dərslər. Sətir yoxdursa bütün dərslər açıq sayılır.</summary>
public class GroupLesson : BaseEntity
{
    public long GroupId { get; set; }

    public Group? Group { get; set; }

    public long LessonId { get; set; }

    public Lesson? Lesson { get; set; }
}
