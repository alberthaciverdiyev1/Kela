using Kela.Application.Features.SiteConfiguration;
using Kela.Domain.Entities;
using Kela.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kela.Infrastructure.Repositories;

internal sealed class SiteConfigurationRepository(KelaDbContext context) : ISiteConfigurationRepository
{
    public Task<BaseSiteConfiguration?> GetSingletonAsync(CancellationToken cancellationToken = default)
        => context.BaseSiteConfigurations
            .FirstOrDefaultAsync(c => c.Id == BaseSiteConfiguration.SingletonId, cancellationToken);

    public void Add(BaseSiteConfiguration config) => context.BaseSiteConfigurations.Add(config);

    public void Update(BaseSiteConfiguration config) => context.BaseSiteConfigurations.Update(config);
}
