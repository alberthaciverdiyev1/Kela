using Kela.Nodes.Shared.Encoding;

namespace Kela.Core.Modules.Nodes;

/// <summary>Nodes koordinatorunun konfiqurasiyası ("Nodes" bölməsi).</summary>
public class NodeOptions
{
    public const string SectionName = "Nodes";

    public int HeartbeatSeconds { get; set; } = 20;

    public int PollSeconds { get; set; } = 10;

    /// <summary>Bu müddət heartbeat göndərməyən cihaz offline sayılır.</summary>
    public int OfflineAfterSeconds { get; set; } = 90;

    /// <summary>Tapşırığın bir cihazda kilidli qaldığı müddət.</summary>
    public int LeaseMinutes { get; set; } = 30;

    public int MaxAttempts { get; set; } = 5;

    /// <summary>
    /// Bir video üçün tam konversiya dövrünün maksimum təkrarı. Keçici şəbəkə xətası
    /// bütün pillələri uğursuz etsə, video dərhal "failed" olmur; bu qədər dövr
    /// yenidən sınanır. Qalıcı xətada təkrar edilmir.
    /// </summary>
    public int MaxVideoAttempts { get; set; } = 3;

    /// <summary>Yarımçıq videoları yoxlayan fon xidmətinin intervalı.</summary>
    public int ReconcileSeconds { get; set; } = 60;

    public int UploadUrlExpiryMinutes { get; set; } = 180;

    public int DownloadUrlExpiryMinutes { get; set; } = 360;

    public int MaxConcurrentJobsPerNode { get; set; } = 4;

    /// <summary>Video yüklənən kimi tapşırıqlar avtomatik yaradılsın.</summary>
    public bool AutoCreateJobs { get; set; } = true;

    /// <summary>Lease/reconciliation fon dövrü işləsin. Testlərdə söndürülür.</summary>
    public bool BackgroundEnabled { get; set; } = true;

    public int SegmentSeconds { get; set; } = VideoLadder.SegmentSeconds;

    public List<EncodingProfile> Ladder { get; set; } = VideoLadder.Default();
}
