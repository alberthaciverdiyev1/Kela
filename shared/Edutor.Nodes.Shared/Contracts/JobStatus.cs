namespace Edutor.Nodes.Shared.Contracts;

/// <summary>
/// Video konversiya tapşırığının həyat dövrü. Keçidlər yalnız koordinator tərəfindən edilir;
/// worker yalnız "Downloading/Processing/Uploading" mərhələlərini bildirir.
/// </summary>
public static class JobStatus
{
    /// <summary>Tapşırıq yaranıb, kuyrukda gözləyir.</summary>
    public const string Pending = "pending";

    /// <summary>Scheduler onu worker-a uyğun görüb növbəyə salıb.</summary>
    public const string Queued = "queued";

    /// <summary>Worker-a verilib, lease aktivdir.</summary>
    public const string Assigned = "assigned";

    public const string Downloading = "downloading";
    public const string Processing = "processing";
    public const string Uploading = "uploading";

    /// <summary>Çıxışlar yoxlanılır (seqmentlər tamdır?).</summary>
    public const string Validating = "validating";

    public const string Completed = "completed";
    public const string Failed = "failed";
    public const string Cancelled = "cancelled";

    public static readonly string[] All =
    [
        Pending, Queued, Assigned, Downloading, Processing, Uploading, Validating, Completed, Failed, Cancelled
    ];

    /// <summary>
    /// Lease bitəndə yenidən kuyruğa alına bilən mərhələlər. <see cref="Validating"/>
    /// da buradadır: koordinator doğrulama zamanı çöksə və ya worker cavabı ala bilməsə,
    /// tapşırıq əbədi "validating" vəziyyətində qalıb videonu bloklayardı.
    /// </summary>
    public static readonly string[] Reclaimable = [Assigned, Downloading, Processing, Uploading, Validating];

    /// <summary>Hələ iş görülən mərhələlər (reconciliation bunları "aktiv" sayır).</summary>
    public static readonly string[] Active = [Pending, Queued, Assigned, Downloading, Processing, Uploading, Validating];

    /// <summary>Terminal mərhələlər.</summary>
    public static readonly string[] Terminal = [Completed, Failed, Cancelled];

    public static bool IsActive(string status) => Array.IndexOf(Active, status) >= 0;

    public static bool IsTerminal(string status) => Array.IndexOf(Terminal, status) >= 0;
}
