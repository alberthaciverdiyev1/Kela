using Kela.Core.Modules.Nodes.Entities;
using Kela.Nodes.Shared.Contracts;

namespace Kela.Core.Modules.Nodes.Abstractions;

/// <summary>
/// Tapşırıqların yaradılması, cihazlara paylanması və lease idarəsi.
/// Bütün əməliyyatlar eyni vaxtda işləyən bir neçə API nüsxəsinə davamlıdır.
/// </summary>
public interface IVideoJobScheduler
{
    /// <summary>Video üçün çatışmayan pillə tapşırıqlarını yaradır (təkrar yaratmır).</summary>
    Task EnsureJobsAsync(long videoId, CancellationToken ct = default);

    /// <summary>Lease-i bitmiş tapşırıqları geri qaytarır və offline cihazları işarələyir.</summary>
    Task ReapAsync(CancellationToken ct = default);

    /// <summary>İşsiz qalmış videoları tapıb tapşırıq yaradır. Finalizasiya tələb edən video ID-lərini qaytarır.</summary>
    Task<IReadOnlyList<long>> ReconcileAsync(CancellationToken ct = default);

    /// <summary>Cihaza uyğun tapşırıqları kilidləyib (lease) qaytarır.</summary>
    /// <param name="preferVideoId">
    /// Verilərsə, əvvəlcə həmin videonun tapşırıqları verilir — böyük bir videonu
    /// bitirmək üçün bütün cihaz gücü ona yönəldilir.
    /// </param>
    Task<IReadOnlyList<JobAssignment>> ClaimAsync(
        WorkerNode node, int max, long? preferVideoId = null, CancellationToken ct = default);
}
