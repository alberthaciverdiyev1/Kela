using System.Text.Json;
using System.Text.Json.Serialization;
using Kela.Nodes.Shared.Contracts;

namespace Kela.Nodes.Worker;

/// <summary>İstifadəçinin tənzimlədiyi və diskdə saxlanılan worker konfiqurasiyası.</summary>
public sealed class WorkerSettings
{
    public string ApiUrl { get; set; } = "http://localhost:5080";

    /// <summary>Qeydiyyatdan sonra alınan access token. Açıq mətn kimi yalnız bu cihazda saxlanılır.</summary>
    public string AccessToken { get; set; } = string.Empty;

    public long NodeId { get; set; }

    public string NodeName { get; set; } = Environment.MachineName;

    public string CacheDirectory { get; set; } = DefaultCacheDirectory();

    public NodeRuntimeLimits Limits { get; set; } = new();

    /// <summary>İşləyən konversiya bitəndən sonra ara fayllar silinsin.</summary>
    public bool CleanupAfterJob { get; set; } = true;

    /// <summary>Tətbiq açılanda worker özü işə düşsün.</summary>
    public bool AutoStart { get; set; }

    /// <summary>
    /// Bu müddətdən qısa VƏ 480p-dən kiçik videolar "yüngül" sayılır: ağır iş
    /// getmirsə, onlardan bir neçəsini paralel aparmaq olar.
    /// </summary>
    public int LightJobMaxSeconds { get; set; } = 300;

    /// <summary>Yalnız yüngül işlər gedərkən paralelliyin neçə dəfə artırılacağı.</summary>
    public int LightJobMultiplier { get; set; } = 2;

    /// <summary>
    /// Son benchmark nəticəsi. Encoder dəyişməyibsə yenidən ölçülmür — sınaq
    /// hər açılışda 10-20 saniyə encoding vaxtı alır.
    /// </summary>
    public NodeBenchmarkResult? Benchmark { get; set; }

    /// <summary>FFmpeg yolu. Boşdursa PATH-dən tapılır.</summary>
    public string FfmpegPath { get; set; } = string.Empty;

    public string FfprobePath { get; set; } = string.Empty;

    /// <summary>Hansı encoder seçilsin. Boşdursa avtomatik (ən yaxşı mövcud).</summary>
    public string PreferredEncoder { get; set; } = string.Empty;

    [JsonIgnore]
    public bool IsRegistered => !string.IsNullOrWhiteSpace(AccessToken) && NodeId > 0;

    public static string DefaultCacheDirectory()
        => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KelaNodes", "cache");

    /// <summary>
    /// Ayar faylının yeri. Testlər öz müvəqqəti faylını verir — əks halda test
    /// işə düşəndə istifadəçinin real konfiqurasiyasını (və tokenini) üzərinə yazır.
    /// </summary>
    [JsonIgnore]
    public string SettingsPath { get; set; } = DefaultSettingsPath();

    public static string DefaultSettingsPath()
        => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KelaNodes", "settings.json");

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public static WorkerSettings Load(string? path = null)
    {
        var target = path ?? DefaultSettingsPath();

        try
        {
            if (File.Exists(target))
            {
                var json = File.ReadAllText(target);
                var loaded = JsonSerializer.Deserialize<WorkerSettings>(json, Options);

                if (loaded is not null)
                {
                    loaded.SettingsPath = target;
                    return loaded;
                }
            }
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            // Konfiqurasiya oxunmasa default-larla davam edilir — worker yenə işə düşsün.
        }

        return new WorkerSettings { SettingsPath = target };
    }

    public void Save()
    {
        var directory = Path.GetDirectoryName(SettingsPath);
        if (string.IsNullOrEmpty(directory)) return;

        Directory.CreateDirectory(directory);

        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(this, Options));

        // Token açıq mətn saxlanılır, ona görə fayl yalnız sahibi üçün oxunaqlı olsun.
        if (!OperatingSystem.IsWindows())
        {
            File.SetUnixFileMode(SettingsPath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
    }
}
