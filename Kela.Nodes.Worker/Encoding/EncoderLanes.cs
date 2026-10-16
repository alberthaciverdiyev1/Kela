using Kela.Nodes.Worker.Device;

namespace Kela.Nodes.Worker.Encoding;

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

    /// <summary>UI-da göstərmək üçün qısa ad.</summary>
    public string Label => Hardware
        ? $"GPU {GpuIndex}·{Slot} nvenc"
        : $"CPU {Slot} x264";
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

        // CPU-ya ayrılan pay çıxıldıqdan sonra qalan büdcə GPU-lara paylanır —
        // beləliklə hər iki qurğu eyni vaxtda işləyir.
        var hardware = capabilities.Chosen.Name == "h264_nvenc";
        var gpuLanes = 0;

        if (hardware)
        {
            var usableGpus = Math.Clamp(gpuCount, 1, 4);
            gpuLanes = Math.Max(usableGpus, budget - cpuLanes);
            gpuLanes = Math.Min(gpuLanes, usableGpus * HardwareLimits.MaxConcurrentNvencSessions);

            var perGpu = Math.Max(1, gpuLanes / usableGpus);
            var slotNumber = 0;

            for (var index = 0; index < usableGpus; index++)
            {
                for (var slot = 0; slot < perGpu && lanes.Count(l => l.Hardware) < gpuLanes; slot++)
                {
                    lanes.Add(new EncoderLane("h264_nvenc", "cuda", true, index, profile.NvencPreset, false)
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
