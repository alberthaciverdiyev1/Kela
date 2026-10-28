using Edutor.Core.Common.Entities;

namespace Edutor.Core.Modules.Auth.Entities;

public class StudentDevice : BaseEntity
{
    public long StudentId { get; set; }

    public string DeviceId { get; set; } = string.Empty;
}
