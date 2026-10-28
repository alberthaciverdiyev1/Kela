namespace Edutor.Nodes.Shared.Contracts;

/// <summary>Cihazın <b>aşkarlanmış</b> xüsusiyyətləri — istifadəçi limitləri deyil.</summary>
public sealed class NodeDeviceInfo
{
    public string OperatingSystem { get; set; } = string.Empty;
    public string OsVersion { get; set; } = string.Empty;
    public string Architecture { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;

    public string? CpuModel { get; set; }
    public int CpuCores { get; set; }
    public int TotalRamMb { get; set; }
    public int AvailableRamMb { get; set; }

    /// <summary>Ekran kartı adları (bilinmirsə boş).</summary>
    public List<string> Gpus { get; set; } = [];

    /// <summary>FFmpeg-in dəstəklədiyi hwaccel siyahısı: cuda, qsv, vaapi, videotoolbox, amf…</summary>
    public List<string> HardwareAcceleration { get; set; } = [];

    /// <summary>Real sınaqdan keçmiş (yoxlanılmış) encoder adları.</summary>
    public List<string> Encoders { get; set; } = [];

    /// <summary>Seçilmiş encoder aparat tərəfindən dəstəklənirmi.</summary>
    public bool HardwareEncoding { get; set; }

    public int DiskFreeMb { get; set; }
    public int DiskTotalMb { get; set; }

    public int? BatteryPercent { get; set; }
    public bool? Charging { get; set; }
    public double? TemperatureCelsius { get; set; }

    public string? FfmpegVersion { get; set; }
    public string? FfprobeVersion { get; set; }
}

/// <summary>İstifadəçinin təyin etdiyi resurs limitləri — scheduler bunları nəzərə alır.</summary>
public sealed class NodeRuntimeLimits
{
    /// <summary>
    /// auto | max | balanced | low. <c>auto</c> olduqda worker cihazın ölçülmüş
    /// gücünə uyğun profili özü seçir və aşağıdaki dəyərlərə də əməl edir.
    /// </summary>
    public string Profile { get; set; } = NodePerformanceProfile.Auto;

    /// <summary>
    /// Açıq limit. <c>0</c> = seçilmiş resurs profili özü qərar verir.
    /// </summary>
    public int MaxConcurrentJobs { get; set; }

    /// <summary>FFmpeg üçün maksimum CPU axını (-threads). 0 = profil / avtomatik.</summary>
    public int MaxCpuThreads { get; set; }

    /// <summary>Keş üçün maksimum disk (MB) — bu hədd aşılsa köhnə iş qovluqları silinir.</summary>
    public int MaxCacheMb { get; set; } = 20480;

    public bool WifiOnly { get; set; }
    public bool ChargingOnly { get; set; }
    public bool PauseOnLowBattery { get; set; } = true;

    /// <summary>Aşağı batareya həddi (%).</summary>
    public int LowBatteryPercent { get; set; } = 20;

    public bool PauseOnHighTemperature { get; set; } = true;

    /// <summary>İstilik həddi (°C).</summary>
    public double MaxTemperatureCelsius { get; set; } = 45;

    /// <summary>Gecə rejimi bitmə saatı — işə salınma (0 = söndürülüb).</summary>
    public int ActiveHoursStart { get; set; }
    public int ActiveHoursEnd { get; set; }
}

/// <summary>Real encoding sınağının nəticəsi.</summary>
public sealed class NodeBenchmarkResult
{
    public string Encoder { get; set; } = string.Empty;

    /// <summary>Ölçülmüş encoding sürəti (saniyə video / saniyə vaxt).</summary>
    public double Fps { get; set; }

    /// <summary>Normalizə edilmiş bal — scheduler güclü/zəif cihazı bununla müqayisə edir.</summary>
    public int Score { get; set; }

    public int TestHeight { get; set; }
    public int TestSeconds { get; set; }
    public DateTimeOffset RanAt { get; set; } = DateTimeOffset.UtcNow;
}
