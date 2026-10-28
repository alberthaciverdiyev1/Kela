using Edutor.Nodes.Worker.Device;
using Edutor.Nodes.Worker.Encoding;

namespace Edutor.Tests.Nodes;

/// <summary>
/// Kodlaşdırma zolaqları: bütün GPU-lar VƏ CPU eyni vaxtda işləməlidir.
/// </summary>
public class LanePlannerTests
{
    private static ProfileSettings Profile(int maxConcurrentJobs = 8, int cpuThreads = 16) =>
        new("max", cpuThreads, maxConcurrentJobs, "veryfast", "p1", false, "test");

    private static EncoderCapabilities Capabilities(bool withCpu = true, string chosen = "h264_nvenc")
    {
        var available = new List<EncoderChoice>();

        if (chosen == "h264_nvenc") available.Add(new EncoderChoice("h264_nvenc", "cuda", true));
        if (withCpu) available.Add(new EncoderChoice("libx264", "cpu", false));

        return new EncoderCapabilities(
            available.First(c => c.Name == chosen),
            available,
            chosen == "h264_nvenc" ? ["cuda"] : []);
    }

    [Fact]
    public void Cpu_lanes_run_alongside_the_gpu()
    {
        var lanes = LanePlanner.Plan(Profile(), Capabilities(), gpuCount: 1, cores: 16);

        Assert.Contains(lanes, l => l.Hardware);
        Assert.Contains(lanes, l => !l.Hardware);
    }

    [Fact]
    public void The_gpu_takes_the_bulk_and_the_cpu_gets_a_share()
    {
        var lanes = LanePlanner.Plan(Profile(), Capabilities(), gpuCount: 1, cores: 16);

        var gpu = lanes.Count(l => l.Hardware);
        var cpu = lanes.Count(l => !l.Hardware);

        Assert.Equal(8, lanes.Count);       // profil büdcəsi
        Assert.Equal(2, cpu);               // 16 nüvə / 8
        Assert.Equal(6, gpu);               // qalan büdcə GPU-ya
        Assert.True(gpu > cpu);
    }

    [Fact]
    public void Every_gpu_gets_its_own_lanes()
    {
        var lanes = LanePlanner.Plan(Profile(), Capabilities(), gpuCount: 2, cores: 16);

        Assert.Equal([0, 1], lanes.Where(l => l.Hardware).Select(l => l.GpuIndex).Distinct().Order());
        Assert.All(lanes.Where(l => l.Hardware), lane => Assert.Contains(lane.GpuIndex, new[] { 0, 1 }));
    }

    [Fact]
    public void Without_a_hardware_encoder_only_cpu_lanes_are_planned()
    {
        var lanes = LanePlanner.Plan(Profile(), Capabilities(chosen: "libx264"), gpuCount: 0, cores: 16);

        Assert.All(lanes, lane => Assert.False(lane.Hardware));
        Assert.NotEmpty(lanes);
    }

    [Fact]
    public void A_machine_without_a_cpu_fallback_still_plans_the_gpu()
    {
        var lanes = LanePlanner.Plan(Profile(), Capabilities(withCpu: false), gpuCount: 1, cores: 16);

        Assert.All(lanes, lane => Assert.True(lane.Hardware));
        Assert.NotEmpty(lanes);
    }

    [Fact]
    public void Lanes_are_distinct_so_each_job_gets_its_own_device()
    {
        var lanes = LanePlanner.Plan(Profile(), Capabilities(), gpuCount: 2, cores: 16);

        // Eyni zolaq iki işə verilə bilməz — CPU zolaqları sıra nömrəsi ilə fərqlənir.
        Assert.Equal(lanes.Count, lanes.Distinct().Count());
    }
}
