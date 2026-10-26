using Kela.Core.Common.Entities;

namespace Kela.Core.Modules.Group.Entities;

using Subject = Kela.Core.Modules.Subject.Entities.Subject;

public class GroupSubject : BaseEntity
{
    public long GroupId { get; set; }

    public Group? Group { get; set; }

    public long SubjectId { get; set; }

    public Subject? Subject { get; set; }

    /// <summary>
    /// Dərs seçimi edilibmi. false → bütün dərslər açıqdır (köhnə qruplar),
    /// true → yalnız group_lessons sətirləri açıqdır (boş ola bilər).
    /// </summary>
    public bool LessonsConfigured { get; set; }
}
