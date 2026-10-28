using Edutor.Nodes.Shared.Contracts;
using Edutor.Nodes.Shared.Encoding;
using Edutor.Nodes.Worker.Device;
using Edutor.Nodes.Worker.Encoding;

namespace Edutor.Tests.Nodes;

/// <summary>
/// Resurs profili: güclü masaüstü tam gücü, zəif/telefon cihaz isə minimum
/// narahatlığı almalıdır.
/// </summary>
public class PerformanceProfileTests
{
    private static readonly EncoderCapabilities HardwareEncoder =
        new(new EncoderChoice("h264_nvenc", "cuda", true),
            [new EncoderChoice("h264_nvenc", "cuda", true), new EncoderChoice("libx264", "cpu", false)],
            ["cuda"]);

    private static readonly EncoderCapabilities CpuEncoder =
        new(new EncoderChoice("libx264", "cpu", false),
            [new EncoderChoice("libx264", "cpu", false)],
            []);

    private static NodeDeviceInfo Device(int cores, bool battery = false, bool charging = false) => new()
    {
        OperatingSystem = battery ? "Android" : "Linux",
        CpuCores = cores,
        BatteryPercent = battery ? 80 : null,
        Charging = battery ? charging : null
    };

    [Fact]
    public void A_strong_desktop_with_a_hardware_encoder_gets_the_maximum_profile()
    {
        var profile = ProfileAdvisor.Resolve(NodePerformanceProfile.Auto, Device(16), HardwareEncoder, new NodeRuntimeLimits());

        Assert.Equal(NodePerformanceProfile.Max, profile.Profile);
        Assert.Equal(16, profile.CpuThreads);
        Assert.True(profile.MaxConcurrentJobs > 1);
        Assert.False(profile.BelowNormalPriority);
    }

    [Fact]
    public void A_phone_on_battery_gets_the_low_profile()
    {
        var profile = ProfileAdvisor.Resolve(NodePerformanceProfile.Auto, Device(8, battery: true), HardwareEncoder, new NodeRuntimeLimits());

        Assert.Equal(NodePerformanceProfile.Low, profile.Profile);
        Assert.Equal(4, profile.CpuThreads);
        Assert.Equal(1, profile.MaxConcurrentJobs);
        Assert.True(profile.BelowNormalPriority);
    }

    [Fact]
    public void A_small_machine_gets_the_low_profile_even_on_mains_power()
    {
        var profile = ProfileAdvisor.Resolve(NodePerformanceProfile.Auto, Device(4), CpuEncoder, new NodeRuntimeLimits());

        Assert.Equal(NodePerformanceProfile.Low, profile.Profile);
    }

    [Fact]
    public void An_average_desktop_lands_on_balanced()
    {
        var profile = ProfileAdvisor.Resolve(NodePerformanceProfile.Auto, Device(8), CpuEncoder, new NodeRuntimeLimits());

        Assert.Equal(NodePerformanceProfile.Balanced, profile.Profile);
        Assert.Equal(7, profile.CpuThreads);   // bir nüvə tətbiqə qalır
    }

    [Fact]
    public void The_chosen_profile_can_be_forced_by_the_user()
    {
        var profile = ProfileAdvisor.Resolve(NodePerformanceProfile.Max, Device(4, battery: true), HardwareEncoder, new NodeRuntimeLimits());

        Assert.Equal(NodePerformanceProfile.Max, profile.Profile);
        Assert.Equal(4, profile.CpuThreads);
    }

    [Fact]
    public void An_explicit_limit_replaces_the_profile_choice()
    {
        var limits = new NodeRuntimeLimits { MaxCpuThreads = 2, MaxConcurrentJobs = 12 };
        var profile = ProfileAdvisor.Resolve(NodePerformanceProfile.Balanced, Device(32), CpuEncoder, limits);

        // İstifadəçi maşını özü tanıyır — açıq limit profili üstələyir (yuxarı da, aşağı da).
        Assert.Equal(2, profile.CpuThreads);
        Assert.Equal(12, profile.MaxConcurrentJobs);
    }

    [Fact]
    public void Nvenc_concurrency_is_capped_at_the_driver_session_limit()
    {
        var limits = new NodeRuntimeLimits { MaxConcurrentJobs = 40 };

        var hardware = ProfileAdvisor.Resolve(NodePerformanceProfile.Max, Device(32), HardwareEncoder, limits);
        var cpu = ProfileAdvisor.Resolve(NodePerformanceProfile.Max, Device(32), CpuEncoder, limits);

        // NVIDIA sürücüsü sessiya limitini aşsa ffmpeg donur — ona görə kəsilir.
        Assert.Equal(HardwareLimits.MaxConcurrentNvencSessions, hardware.MaxConcurrentJobs);

        // CPU kodlaşdırmasında belə limit yoxdur.
        Assert.Equal(40, cpu.MaxConcurrentJobs);
    }

    [Fact]
    public void The_maximum_profile_uses_half_of_the_cores_in_parallel()
    {
        var profile = ProfileAdvisor.Resolve(NodePerformanceProfile.Max, Device(16), HardwareEncoder, new NodeRuntimeLimits());

        Assert.Equal(8, profile.MaxConcurrentJobs);
    }

    [Fact]
    public void The_low_profile_asks_ffmpeg_for_a_slower_hardware_preset()
    {
        var balanced = ProfileAdvisor.Resolve(NodePerformanceProfile.Balanced, Device(16), HardwareEncoder, new NodeRuntimeLimits());
        var low = ProfileAdvisor.Resolve(NodePerformanceProfile.Low, Device(16), HardwareEncoder, new NodeRuntimeLimits());
        var max = ProfileAdvisor.Resolve(NodePerformanceProfile.Max, Device(16), HardwareEncoder, new NodeRuntimeLimits());

        // NVENC preset-lərində kiçik rəqəm daha sürətli deməkdir.
        Assert.Equal("p1", max.NvencPreset);
        Assert.Equal("p4", balanced.NvencPreset);
        Assert.Equal("p6", low.NvencPreset);
    }

    [Fact]
    public void The_resolved_profile_reaches_the_ffmpeg_command_line()
    {
        var profile = ProfileAdvisor.Resolve(NodePerformanceProfile.Low, Device(8), HardwareEncoder, new NodeRuntimeLimits());

        var arguments = EncodePlan.Hls(new EncodeRequest
        {
            Input = "/tmp/in.mp4",
            OutputDirectory = "/tmp/out",
            Profile = new EncodingProfile { Name = "360p", Height = 360, VideoBitrateKbps = 700 },
            Encoder = new EncoderChoice("h264_nvenc", "cuda", true),
            SourceHeight = 1080,
            SourceFps = 25,
            VideoStreamIndex = 0,
            AudioStreamIndex = 1,
            Threads = profile.CpuThreads,
            Preset = profile.NvencPreset,
            BelowNormalPriority = profile.BelowNormalPriority
        }).ToList();

        Assert.Equal("p6", arguments[arguments.IndexOf("-preset") + 1]);
        Assert.Equal("4", arguments[arguments.IndexOf("-threads") + 1]);
    }

    [Fact]
    public void An_unknown_profile_falls_back_to_auto()
    {
        Assert.Equal(NodePerformanceProfile.Auto, NodePerformanceProfile.Normalize("turbo"));
        Assert.Equal(NodePerformanceProfile.Auto, NodePerformanceProfile.Normalize(null));
        Assert.Equal(NodePerformanceProfile.Max, NodePerformanceProfile.Normalize("max"));
    }
}
