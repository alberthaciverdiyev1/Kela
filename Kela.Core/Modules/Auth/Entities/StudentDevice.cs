using Kela.Core.Common.Entities;

namespace Kela.Core.Modules.Auth.Entities;

public class StudentDevice : BaseEntity
{
    public long StudentId { get; set; }

    public string DeviceId { get; set; } = string.Empty;
}
