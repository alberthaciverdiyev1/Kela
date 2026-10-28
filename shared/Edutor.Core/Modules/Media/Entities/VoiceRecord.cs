using Edutor.Core.Common.Entities;

namespace Edutor.Core.Modules.Media.Entities;

public class VoiceRecord : BaseEntity
{
    public string? Title { get; set; }

    public string Path { get; set; } = string.Empty;

    public int? DurationSeconds { get; set; }
}
