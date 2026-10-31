using Edutor.Core.Common.Entities;

namespace Edutor.Core.Modules.TeacherNote.Entities;

public class TeacherNoteShare : BaseEntity
{
    public long TeacherNoteId { get; set; }

    public long UserId { get; set; }
}
