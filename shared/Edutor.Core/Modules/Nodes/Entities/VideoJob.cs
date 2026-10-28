using Edutor.Core.Common.Entities;
using Edutor.Nodes.Shared.Contracts;

namespace Edutor.Core.Modules.Nodes.Entities;

using Video = Edutor.Core.Modules.Media.Entities.Video;

/// <summary>
/// Bir videonun bir keyfiyyət pilləsi üçün konversiya tapşırığı. Hər pillə ayrı
/// tapşırıqdır ki, scheduler onları müxtəlif cihazlara paylaya bilsin.
/// </summary>
public class VideoJob : BaseEntity
{
    public long VideoId { get; set; }

    public Video? Video { get; set; }

    /// <summary>Variant adı — həm də çıxış qovluğu ("720p").</summary>
    public string Resolution { get; set; } = string.Empty;

    public int TargetHeight { get; set; }

    public int TargetBitrateKbps { get; set; }

    /// <summary>bax: <see cref="JobStatus"/>.</summary>
    public string Status { get; set; } = JobStatus.Pending;

    /// <summary>0..1</summary>
    public double Progress { get; set; }

    public int Attempts { get; set; }

    public int MaxAttempts { get; set; } = 5;

    public long? WorkerNodeId { get; set; }

    public WorkerNode? WorkerNode { get; set; }

    /// <summary>Fencing token — hər cəhddə yenilənir; köhnə cəhdin nəticəsi rədd edilir.</summary>
    public string? AttemptToken { get; set; }

    public DateTimeOffset? LeaseExpiresAt { get; set; }

    /// <summary>Exponential backoff — bu vaxtdan əvvəl yenidən verilmir.</summary>
    public DateTimeOffset? NextAttemptAt { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public string? ErrorMessage { get; set; }

    /// <summary>transient | permanent — bax: <see cref="JobErrorKind"/>.</summary>
    public string? ErrorKind { get; set; }

    /// <summary>Çıxışların yazıldığı bucket prefiksi (versiya ilə).</summary>
    public string OutputPrefix { get; set; } = string.Empty;

    /// <summary>Variant playlist açarı (prefiksə nisbətən).</summary>
    public string VariantKey { get; set; } = string.Empty;

    public int? SourceHeight { get; set; }

    public int? SourceWidth { get; set; }

    public double? SourceFps { get; set; }

    public bool HasAudio { get; set; } = true;

    /// <summary>Böyük = əvvəl işlənir.</summary>
    public int Priority { get; set; }

    public double? EncodingFps { get; set; }

    public string? Encoder { get; set; }

    /// <summary>Mənbə bu pillədən kiçik olduğu üçün buraxıldı (upscale etməmək üçün).</summary>
    public bool Skipped { get; set; }
}
