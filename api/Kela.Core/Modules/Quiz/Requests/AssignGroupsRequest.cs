namespace Kela.Core.Modules.Quiz.Requests;

public class AssignGroupsRequest
{
    public List<long> GroupIds { get; set; } = new();
}
