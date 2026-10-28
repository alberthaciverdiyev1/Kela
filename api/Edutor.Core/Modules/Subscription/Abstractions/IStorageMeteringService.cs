namespace Edutor.Core.Modules.Subscription.Abstractions;

/// <summary>Tenant-ların bucket-da tutduğu yeri ölçür və (konfiqurasiya ilə) köhnə orijinalları silir.</summary>
public interface IStorageMeteringService
{
    Task<MeteringResult> MeterAsync(CancellationToken ct = default);
}

public sealed record MeteringResult(int Tenants, long TotalBytes, int DeletedOriginals);
