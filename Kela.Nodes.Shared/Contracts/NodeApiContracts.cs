namespace Kela.Nodes.Shared.Contracts;

// ── Qeydiyyat ────────────────────────────────────────────────────────────────

/// <summary>
/// Cihazın ilk qoşulması. <see cref="RegistrationCode"/> admin paneldən verilir və
/// birdəfəlikdir — ondan sonra cihaz öz access token-i ilə işləyir.
/// </summary>
public sealed class NodeRegisterRequest
{
    public string RegistrationCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public NodeDeviceInfo Device { get; set; } = new();
}

public sealed class NodeRegisterResponse
{
    public long NodeId { get; set; }

    /// <summary>Uzunömürlü access token. Yalnız bu cavabda açıq göndərilir.</summary>
    public string AccessToken { get; set; } = string.Empty;

    public int HeartbeatSeconds { get; set; }
    public int PollSeconds { get; set; }
    public int MaxConcurrentJobs { get; set; }
}

/// <summary>Token rotasiyası — köhnə token dərhal etibarsız olur.</summary>
public sealed class NodeRotateTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
}

// ── Heartbeat ────────────────────────────────────────────────────────────────

public sealed class NodeHeartbeatRequest
{
    public NodeDeviceInfo? Device { get; set; }
    public NodeRuntimeLimits? Limits { get; set; }
    public NodeBenchmarkResult? Benchmark { get; set; }

    /// <summary>Hazırda işlənən tapşırıqlar və onların lokal vəziyyəti.</summary>
    public List<NodeJobReport> Jobs { get; set; } = [];
}

public sealed class NodeJobReport
{
    public long JobId { get; set; }
    public string AttemptToken { get; set; } = string.Empty;
    public string Status { get; set; } = JobStatus.Processing;
    public double Progress { get; set; }
    public string? Message { get; set; }
}

public sealed class NodeHeartbeatResponse
{
    public DateTimeOffset ServerTime { get; set; }

    /// <summary>Server tərəfdə hələ də bu cihaza aid sayılan tapşırıqlar.</summary>
    public List<long> ActiveJobIds { get; set; } = [];

    /// <summary>Ləğv edilməli tapşırıqlar (lease itirildi və ya admin ləğv etdi).</summary>
    public List<long> CancelJobIds { get; set; } = [];

    public int HeartbeatSeconds { get; set; }
    public int PollSeconds { get; set; }
}

// ── Tapşırıq alma ────────────────────────────────────────────────────────────

public sealed class JobClaimRequest
{
    /// <summary>Bu çağırışda maksimum neçə tapşırıq istənilir.</summary>
    public int Max { get; set; } = 1;
}

/// <summary>Worker-a verilən tapşırıq təsviri.</summary>
public sealed class JobAssignment
{
    public long JobId { get; set; }
    public long VideoId { get; set; }
    public string Resolution { get; set; } = string.Empty;

    /// <summary>Fencing token — hər cəhddə dəyişir; köhnə cəhdin nəticəsi bununla rədd edilir.</summary>
    public string AttemptToken { get; set; } = string.Empty;

    public int Attempt { get; set; }

    /// <summary>Orijinal videonu endirmək üçün qısaömürlü URL (presigned GET).</summary>
    public string SourceUrl { get; set; } = string.Empty;
    public string SourceKey { get; set; } = string.Empty;

    /// <summary>Çıxışların yazılacağı bucket prefiksi.</summary>
    public string OutputPrefix { get; set; } = string.Empty;

    /// <summary>Master playlist-in yazılacağı tam açar (koordinator yaradır).</summary>
    public string MasterKey { get; set; } = string.Empty;

    public int? DurationSeconds { get; set; }
    public int? SourceHeight { get; set; }
    public int? SourceWidth { get; set; }
    public double? SourceFps { get; set; }
    public bool HasAudio { get; set; } = true;

    /// <summary>Çıxarılacaq keyfiyyət pilləsi — yalnız bu resolution kodlaşdırılır.</summary>
    public Encoding.EncodingProfile Profile { get; set; } = new();

    /// <summary>Master playlist-də yazılacaq bütün variantlar (worker onları yaratmır, koordinator yazır).</summary>
    public List<Encoding.EncodingProfile> Ladder { get; set; } = [];

    /// <summary>Lease bitmə vaxtı — bundan sonra koordinator tapşırığı başqasına verə bilər.</summary>
    public DateTimeOffset LeaseExpiresAt { get; set; }
}

// ── İrəliləyiş / nəticə ──────────────────────────────────────────────────────

/// <summary>Mənbə videonun ffprobe nəticəsi — koordinator merdiveni buna görə düzəldir.</summary>
public sealed class SourceMediaInfo
{
    public int Width { get; set; }
    public int Height { get; set; }
    public double? Fps { get; set; }
    public int? DurationSeconds { get; set; }
    public bool HasAudio { get; set; } = true;
}

public sealed class JobProgressRequest
{
    public string AttemptToken { get; set; } = string.Empty;

    /// <summary>Yalnız ilk probe-da göndərilir.</summary>
    public SourceMediaInfo? Media { get; set; }
    public string Status { get; set; } = JobStatus.Processing;

    /// <summary>0..1</summary>
    public double Progress { get; set; }

    public double? Fps { get; set; }
    public double? Speed { get; set; }
    public int? EtaSeconds { get; set; }
    public string? Message { get; set; }

    /// <summary>Lease-i uzatmaq üçün — uzun kodlaşdırmalarda worker bunu göndərir.</summary>
    public bool ExtendLease { get; set; } = true;
}

/// <summary>Yüklənəcək obyektlər üçün imza istəyi.</summary>
public sealed class JobUploadUrlRequest
{
    public string AttemptToken { get; set; } = string.Empty;

    /// <summary>Output prefiksinə nisbətən açarlar (məs. "720p/segment_00001.ts").</summary>
    public List<string> Keys { get; set; } = [];
}

public sealed class JobUploadUrlResponse
{
    /// <summary>açar → URL. <see cref="Proxy"/> true olduqda bunlar API-nin öz proxysidir.</summary>
    public Dictionary<string, string> Urls { get; set; } = [];

    /// <summary>
    /// true = storage presigned PUT dəstəkləmir (lokal rejim); URL-lər API-yə işarə edir
    /// və gövdə birbaşa ora göndərilir.
    /// </summary>
    public bool Proxy { get; set; }

    public int ExpiresSeconds { get; set; }
}

public sealed class JobCompleteRequest
{
    public string AttemptToken { get; set; } = string.Empty;

    public int? DurationSeconds { get; set; }

    /// <summary>Output prefiksinə nisbətən thumbnail açarı.</summary>
    public string? ThumbnailKey { get; set; }

    /// <summary>Yazılmış bütün açarlar (output prefiksinə nisbətən) — koordinator onları yoxlayır.</summary>
    public List<string> Objects { get; set; } = [];

    /// <summary>Bu tapşırığın variant playlist açarı (məs. "720p/index.m3u8").</summary>
    public string VariantKey { get; set; } = string.Empty;

    public double? Fps { get; set; }
    public string? Encoder { get; set; }
}

public sealed class JobFailRequest
{
    public string AttemptToken { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;

    /// <summary>transient | permanent — bax: <see cref="JobErrorKind"/>.</summary>
    public string Kind { get; set; } = JobErrorKind.Transient;
}
