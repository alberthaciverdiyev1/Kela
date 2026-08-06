using Kela.Domain.Entities;

namespace Kela.Application.Features.SiteConfiguration;

public interface ISiteConfigurationRepository
{
    /// <summary>Singleton satırı getirir (yoksa null).</summary>
    Task<BaseSiteConfiguration?> GetSingletonAsync(CancellationToken cancellationToken = default);

    void Add(BaseSiteConfiguration config);
    void Update(BaseSiteConfiguration config);
}
