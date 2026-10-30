using Edutor.Core.Common.Entities;

namespace Edutor.Core.Modules.TeacherNote.Entities;

/// <summary>Müəllimin şəxsi qeydi. Tarix verilirsə təqvimdə də görünür.</summary>
public class TeacherNote : BaseEntity
{
    public long UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Body { get; set; }

    public DateOnly? Date { get; set; }

    public bool IsDone { get; set; }
}
