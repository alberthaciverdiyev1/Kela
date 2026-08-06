using Kela.Domain.Common;

namespace Kela.Domain.Entities;
public class City : BaseEntity
{
    public Dictionary<string, string> NameTranslations { get; set; } = new();
}
