using Kela.Core.Common.Entities;

namespace Kela.Core.Modules.Media.Entities;

public class MediaFile : BaseEntity
{
    public string? Name { get; set; }

    public string Path { get; set; } = string.Empty;

    public long? SizeBytes { get; set; }
}
