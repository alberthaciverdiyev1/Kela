namespace Edutor.Core.Modules.Group.Requests;

public class AssignSubjectsRequest
{
    public List<long> SubjectIds { get; set; } = new();

    /// <summary>
    /// Fənnlə birlikdə açılacaq dərslər. null → dərs seçimi dəyişdirilmir (köhnə davranış),
    /// boş siyahı → fənn açılır, heç bir dərs göstərilmir.
    /// </summary>
    public List<long>? LessonIds { get; set; }
}
