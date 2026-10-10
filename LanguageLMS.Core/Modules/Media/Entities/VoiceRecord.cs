using LanguageLMS.Core.Common.Entities;

namespace LanguageLMS.Core.Modules.Media.Entities;

public class VoiceRecord : BaseEntity
{
    public string? Title { get; set; }

    public string Path { get; set; } = string.Empty;

    public int? DurationSeconds { get; set; }
}
