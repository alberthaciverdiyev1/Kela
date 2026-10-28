using Edutor.Nodes.Shared.Contracts;
using Edutor.Nodes.Shared.Encoding;
using Edutor.Nodes.Worker.Device;
using Edutor.Nodes.Worker.Jobs;

namespace Edutor.Tests.Nodes;

/// <summary>
/// Paralellik: yalnız qısa/kiçik işlər gedirsə daha çoxunu paralel apar,
/// ağır iş başlayanda profil həddinə qayıt, yaddaş/disktə yer yoxsa azalt.
/// </summary>
public class ConcurrencyPolicyTests
{
    private static ProfileSettings Profile(int maxConcurrentJobs) =>
        new("balanced", 8, maxConcurrentJobs, "veryfast", "p4", false, "test");

    private static RunningJob Job(bool light, long id = 1)
    {
        var assignment = new JobAssignment
        {
            JobId = id,
            VideoId = id,
            Resolution = "360p",
            AttemptToken = "t",
            Profile = new EncodingProfile { Name = "360p", Height = 360, VideoBitrateKbps = 700 }
        };

        return new RunningJob(assignment) { IsLight = light };
    }

    private const int PlentyRam = 32_000;
    private const int PlentyDisk = 500_000;

    [Fact]
    public void Only_light_jobs_allow_double_the_profile_limit()
    {
        var jobs = new List<RunningJob> { Job(true, 1), Job(true, 2), Job(true, 3), Job(true, 4) };

        var allowance = ConcurrencyPolicy.Allowance(Profile(4), jobs, PlentyRam, PlentyDisk, lightJobMultiplier: 2);

        Assert.Equal(8, allowance);
    }

    [Fact]
    public void A_single_heavy_job_returns_to_the_profile_limit()
    {
        var jobs = new List<RunningJob> { Job(true, 1), Job(false, 2) };

        var allowance = ConcurrencyPolicy.Allowance(Profile(4), jobs, PlentyRam, PlentyDisk, lightJobMultiplier: 2);

        Assert.Equal(4, allowance);
    }

    [Fact]
    public void Low_memory_shrinks_the_allowance()
    {
        var jobs = new List<RunningJob>();

        // 1.4 GB boş → 250 MB/iş → 5 iş, amma profil həddi 8 və yüngül çarpan 2 → 8;
        // yaddaş onu 5-ə endirir.
        var allowance = ConcurrencyPolicy.Allowance(Profile(8), jobs, availableRamMb: 1_400, freeDiskMb: PlentyDisk, lightJobMultiplier: 2);

        Assert.Equal(5, allowance);
    }

    [Fact]
    public void A_full_disk_stops_new_work()
    {
        // 1.5 GB boş disk → yalnız 1 iş (disk hər iş üçün 1 GB tələb edir).
        var allowance = ConcurrencyPolicy.Allowance(Profile(8), [], PlentyRam, freeDiskMb: 1_500, lightJobMultiplier: 2);

        Assert.Equal(1, allowance);
    }

    [Fact]
    public void The_allowance_never_drops_below_the_jobs_already_running()
    {
        var jobs = new List<RunningJob> { Job(false, 1), Job(false, 2), Job(false, 3) };

        var allowance = ConcurrencyPolicy.Allowance(Profile(4), jobs, availableRamMb: 300, freeDiskMb: 400, lightJobMultiplier: 2);

        Assert.Equal(3, allowance);
    }

    [Fact]
    public void Without_a_profile_only_one_job_runs()
    {
        Assert.Equal(1, ConcurrencyPolicy.Allowance(null, [], PlentyRam, PlentyDisk, 2));
    }
}
