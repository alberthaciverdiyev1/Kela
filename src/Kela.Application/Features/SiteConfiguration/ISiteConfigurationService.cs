using Kela.Application.Features.SiteConfiguration.Requests;
using Kela.Application.Features.SiteConfiguration.Responses;

namespace Kela.Application.Features.SiteConfiguration;

public interface ISiteConfigurationService
{
    Task<SiteConfigurationResponse> GetAsync(CancellationToken cancellationToken = default);

    Task UpdateAsync(UpdateSiteConfigurationRequest request, CancellationToken cancellationToken = default);
}
