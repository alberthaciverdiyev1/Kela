using Kela.Nodes.Worker.Jobs;

namespace Kela.Nodes.Worker.Device;

/// <summary>
/// Neçə tapşırığın eyni vaxtda işləyəcəyini müəyyən edir.
///
/// Profil əsas həddi verir, lakin qısa/kiçik videolar bir ffmpeg prosesini uzun
/// müddət məşğul etmir: belə işlər yığılıbsa daha çoxunu paralel aparmaq
/// konversiyanı sürətləndirir. Ağır (uzun və ya böyük) bir iş işə düşən kimi
/// yenidən profil həddinə qayıdılır ki, maşın boğulmasın.
/// </summary>
public static class ConcurrencyPolicy
{
    /// <summary>Bir kodlaşdırma prosesi üçün təxmini RAM ehtiyatı (MB).</summary>
    private const int RamPerJobMb = 700;

    /// <summary>Diskdə hər iş üçün təxmini boş yer tələbi (MB) — mənbə + seqmentlər.</summary>
    private const int DiskPerJobMb = 2 * 1024;

    public static int Allowance(
        ProfileSettings? profile,
        IReadOnlyCollection<RunningJob> jobs,
        int availableRamMb,
        int freeDiskMb,
        int lightJobMultiplier,
        int encoderLanes = 0)
    {
        if (profile is null) return 1;

        var baseLimit = Math.Max(1, profile.MaxConcurrentJobs);
        var running = jobs.Count;
        var heavy = jobs.Count(j => !j.IsLight);

        // Ağır iş gedirsə profil həddi aşılmır.
        var allowed = heavy > 0 ? baseLimit : baseLimit * Math.Max(1, lightJobMultiplier);

        // Hər paralel iş bir kodlaşdırma zolağı tutur (GPU və ya CPU).
        if (encoderLanes > 0) allowed = Math.Min(allowed, encoderLanes);

        // Yaddaş və disk də sözünü deyir — həddi yalnız aşağı sala bilərlər.
        if (availableRamMb > 0) allowed = Math.Min(allowed, Math.Max(1, availableRamMb / RamPerJobMb));
        if (freeDiskMb > 0) allowed = Math.Min(allowed, Math.Max(1, freeDiskMb / DiskPerJobMb));

        // Artıq işləyənlərdən az verə bilməz.
        return Math.Max(running, Math.Max(1, allowed));
    }
}
