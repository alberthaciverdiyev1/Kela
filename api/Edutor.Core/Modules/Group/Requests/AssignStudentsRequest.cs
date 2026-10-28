namespace Edutor.Core.Modules.Group.Requests;

public class AssignStudentsRequest
{
    public List<long> StudentIds { get; set; } = new();

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }
}
