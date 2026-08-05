using Kela.Domain.Common;

namespace Kela.Domain.Subjects;

public class Subject : BaseEntity, ITenantEntity
{
    public int TenantId { get; set; }
    public string? Name { get; set; }

}
