using Edutor.Core.Common.Entities;

namespace Edutor.Core.Modules.ScreenGuard.Entities;

/// <summary>Şagird tərəfində aşkarlanmış ekran qoruması pozuntusu (fokus itkisi, ekran paylaşımı və s.).</summary>
public class ScreenViolation : BaseEntity
{
    public long StudentId { get; set; }

    /// <summary>Pozuntu növü: window-blur | visibility-hidden | display-capture-start | shortcut:* | contextmenu | copy ...</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Hadisənin baş verdiyi URL (app route).</summary>
    public string? Path { get; set; }

    /// <summary>Brauzer user-agent (native tətbiqdə platforma məlumatı).</summary>
    public string? UserAgent { get; set; }
}
