using Edutor.Core.Common.Entities;

namespace Edutor.Core.Modules.Group.Entities;

using Student = Edutor.Core.Modules.Auth.Entities.Student;

public class GroupJoinRequest : BaseEntity
{
    public long GroupId { get; set; }

    public Group? Group { get; set; }

    public long StudentId { get; set; }

    public Student? Student { get; set; }

    /// <summary>pending | approved | rejected</summary>
    public string Status { get; set; } = GroupJoinStatus.Pending;

    public DateTimeOffset? DecidedAt { get; set; }
}

public static class GroupJoinStatus
{
    public const string Pending = "pending";
    public const string Approved = "approved";
    public const string Rejected = "rejected";
}
