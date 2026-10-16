using Kela.Nodes.Shared.Contracts;
using Kela.Nodes.Worker.Encoding;

namespace Kela.Nodes.Worker.Device;

/// <summary>
/// Profildən çıxarılan konkret kodlaşdırma parametrləri.
/// </summary>
public sealed record ProfileSettings(
    string Profile,
    int CpuThreads,
    int MaxConcurrentJobs,
    string Libx264Preset,
    string NvencPreset,
    bool BelowNormalPriority,
    string Description);

/// <summary>
/// Cihazın ölçülmüş gücünə uyğun resurs profilini seçir və konkret parametrlərə çevirir.
/// Məqsəd: güclü masaüstü tam gücü ilə işləsin, zəif/telefon cihaz istifadəçini
/// narahat etməsin.
/// </summary>
public static class ProfileAdvisor
{
    public static ProfileSettings Resolve(
        string requested, NodeDeviceInfo device, EncoderCapabilities? encoders, NodeRuntimeLimits limits)
    {
        var profile = NodePerformanceProfile.Normalize(requested);

        if (profile == NodePerformanceProfile.Auto)
        {
            profile = Choose(device, encoders);
        }

        var cores = Math.Max(1, device.CpuCores);
        var hardware = encoders?.Chosen.Hardware == true;

        var settings = profile switch
        {
            NodePerformanceProfile.Max => new ProfileSettings(
                profile,
                CpuThreads: cores,
                MaxConcurrentJobs: Math.Max(1, cores / 4),
                Libx264Preset: "veryfast",
                NvencPreset: "p1",
                BelowNormalPriority: false,
                Description: $"{cores} nüvənin hamısı, ən sürətli preset"),

            NodePerformanceProfile.Low => new ProfileSettings(
                profile,
                CpuThreads: Math.Max(1, cores / 2),
                MaxConcurrentJobs: 1,
                Libx264Preset: "veryfast",
                NvencPreset: "p6",
                BelowNormalPriority: true,
                Description: $"{Math.Max(1, cores / 2)} nüvə, aşağı prioritet — cihaz istifadəçiyə qalır"),

            _ => new ProfileSettings(
                profile,
                CpuThreads: Math.Max(1, cores - 1),
                MaxConcurrentJobs: Math.Max(1, cores / 6),
                Libx264Preset: "veryfast",
                NvencPreset: "p4",
                BelowNormalPriority: false,
                Description: $"{Math.Max(1, cores - 1)} nüvə, bir nüvə tətbiqə saxlanılır")
        };

        // İstifadəçi açıq limit qoyubsa, profil onu üstələmir.
        if (limits.MaxCpuThreads > 0)
        {
            settings = settings with { CpuThreads = Math.Min(settings.CpuThreads, limits.MaxCpuThreads) };
        }

        if (limits.MaxConcurrentJobs > 0)
        {
            settings = settings with { MaxConcurrentJobs = Math.Min(settings.MaxConcurrentJobs, limits.MaxConcurrentJobs) };
        }

        return hardware
            ? settings
            : settings with { Description = settings.Description + ", CPU kodlaşdırması" };
    }

    /// <summary>
    /// Avtomatik seçim: batareya ilə işləyən və ya zəif cihaz aşağı profil alır,
    /// çoxnüvəli masaüstü aparat encoder-i ilə maksimuma keçir.
    /// </summary>
    private static string Choose(NodeDeviceInfo device, EncoderCapabilities? encoders)
    {
        var cores = device.CpuCores;
        var onBattery = device.BatteryPercent is not null && device.Charging != true;
        var hardware = encoders?.Chosen.Hardware == true;

        // Telefon/planşet: istifadəçini narahat etməmək üçün aşağı profil.
        if (onBattery || cores <= 4) return NodePerformanceProfile.Low;

        // Güclü masaüstü + aparat encoder-i → tam güc.
        if (hardware && cores >= 8) return NodePerformanceProfile.Max;

        return NodePerformanceProfile.Balanced;
    }
}
