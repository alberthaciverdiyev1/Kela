using Kela.Domain.Entities;

namespace Kela.Application.Features.SiteConfiguration;

public interface ISiteConfigurationRepository
{
    Task<BaseSiteConfiguration?> GetSingletonAsync(CancellationToken cancellationToken = default);

    void Add(BaseSiteConfiguration config);
    void Update(BaseSiteConfiguration config);
}
