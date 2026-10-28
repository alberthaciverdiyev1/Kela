using Edutor.Nodes.Worker.Device;

namespace Edutor.Nodes.Worker.Encoding;

/// <summary>
/// Bir kodlaşdırma "zolağı": eyni anda bir iş tuta bilən müstəqil kodlaşdırma qurğusu.
/// Hər GPU üçün bir NVENC zolağı, üstəgəl CPU üçün bir neçə libx264 zolağı.
/// </summary>
public sealed record EncoderLane(
    string Name,
    string HwAccel,
    bool Hardware,
    int GpuIndex,
    string Preset,
    bool BelowNormalPriority)
{
    /// <summary>
    /// Zolağın nömrəsi. Bir qurğu bir neçə zolaq verə bilər (NVENC eyni vaxtda bir
    /// neçə sessiya aça bilir) — nömrə onları bir-birindən fərqləndirir, əks halda
    /// eyni zolaq iki işə verilə bilər.
    /// </summary>
    public int Slot { get; init; }

    /// <summary>UI-da göstərmək üçün qısa ad: "GPU 0·1 nvenc" / "GPU·1 mediacodec" / "CPU 1 x264".</summary>
    public string Label => Hardware
        ? $"{(GpuIndex >= 0 ? $"GPU {GpuIndex}" : "GPU")}·{Slot} {Name}"
        : $"CPU {Slot} {Name}";
}

/// <summary>
/// Mövcud kodlaşdırma qurğularından zolaq siyahısı qurur. Məqsəd: bütün GPU-ları
/// <b>və</b> CPU-nu eyni vaxtda işlətmək — NVENC sürətli, CPU isə boş qalan gücü
/// istifadə edir.
/// </summary>
public static class LanePlanner
{
    public static IReadOnlyList<EncoderLane> Plan(
        ProfileSettings profile,
        EncoderCapabilities capabilities,
        int gpuCount,
        int cores)
    {
        var lanes = new List<EncoderLane>();
        var budget = Math.Max(1, profile.MaxConcurrentJobs);

        var cpuLanes = capabilities.Available.Any(c => c.Name == "libx264") && profile.CpuThreads > 0
            ? Math.Clamp(cores / 8, 1, 4)
            : 0;

        // CPU-ya ayrılan pay çıxıldıqdan sonra qalan büdcə aparat encoder-inə verilir —
        // beləliklə hər iki qurğu eyni vaxtda işləyir.
        var chosen = capabilities.Chosen;
        var hardware = chosen.Hardware;
        var slotNumber = 0;

        if (hardware)
        {
            // NVENC bir kartda bir neçə sessiya aça bilir; MediaCodec isə bir encoder-dir.
            var nvenc = chosen.Name == "h264_nvenc";
            var usableDevices = nvenc ? Math.Clamp(gpuCount, 1, 4) : 1;

            var hardwareLanes = nvenc
                ? Math.Min(Math.Max(usableDevices, budget - cpuLanes), usableDevices * HardwareLimits.MaxConcurrentNvencSessions)
                : 1;

            var perDevice = Math.Max(1, hardwareLanes / usableDevices);

            // Preset yalnız NVENC-də məna daşıyır; digər aparat encoder-ləri öz default-unu işlədir.
            var preset = nvenc ? profile.NvencPreset : string.Empty;

            for (var index = 0; index < usableDevices; index++)
            {
                for (var slot = 0; slot < perDevice && lanes.Count(l => l.Hardware) < hardwareLanes; slot++)
                {
                    lanes.Add(new EncoderLane(chosen.Name, chosen.HwAccel, true, nvenc ? index : -1, preset, false)
                    {
                        Slot = ++slotNumber
                    });
                }
            }
        }

        for (var index = 0; index < cpuLanes; index++)
        {
            lanes.Add(new EncoderLane("libx264", "cpu", false, -1, profile.Libx264Preset, profile.BelowNormalPriority)
            {
                Slot = index + 1
            });
        }

        return lanes.Count > 0
            ? lanes
            : [new EncoderLane(capabilities.Chosen.Name, capabilities.Chosen.HwAccel, capabilities.Chosen.Hardware, -1,
                capabilities.Chosen.Hardware ? profile.NvencPreset : profile.Libx264Preset, profile.BelowNormalPriority)
                { Slot = 1 }];
    }

    /// <summary>CPU zolağının istifadə edəcəyi axın sayı.</summary>
    public static int ThreadsFor(EncoderLane lane, ProfileSettings profile, int cpuLaneCount)
        => lane.Hardware || cpuLaneCount <= 0 ? profile.CpuThreads : Math.Max(1, profile.CpuThreads / cpuLaneCount);
}
