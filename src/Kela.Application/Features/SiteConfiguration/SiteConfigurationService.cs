using FluentValidation;
using Kela.Application.Features.SiteConfiguration.Requests;
using Kela.Application.Features.SiteConfiguration.Responses;
using Kela.Application.Patterns;
using Kela.Domain.Entities;

namespace Kela.Application.Features.SiteConfiguration;

internal sealed class SiteConfigurationService(
    ISiteConfigurationRepository repository,
    IUnitOfWork unitOfWork,
    IValidator<UpdateSiteConfigurationRequest> validator) : ISiteConfigurationService
{
    public async Task<SiteConfigurationResponse> GetAsync(CancellationToken cancellationToken = default)
    {
        var config = await GetOrCreateAsync(cancellationToken);
        return ToResponse(config);
    }

    public async Task UpdateAsync(
        UpdateSiteConfigurationRequest request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var config = await GetOrCreateAsync(cancellationToken);
        config.Update(
            request.SiteName.Trim(),
            request.PrimaryColor, request.SecondaryColor, request.SuccessColor,
            request.WarningColor, request.ErrorColor, request.InfoColor,
            request.NavMode);

        repository.Update(config);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<BaseSiteConfiguration> GetOrCreateAsync(CancellationToken cancellationToken)
    {
        var config = await repository.GetSingletonAsync(cancellationToken);
        if (config is not null)
        {
            return config;
        }

        // Satır yoksa varsayılanlarla oluştur (default değerler entity'de).
        config = new BaseSiteConfiguration
        {
            Id = BaseSiteConfiguration.SingletonId,
            CreatedAt = DateTime.UtcNow,
        };
        repository.Add(config);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return config;
    }

    private static SiteConfigurationResponse ToResponse(BaseSiteConfiguration c)
        => new(
            c.SiteName,
            c.PrimaryColor,
            c.SecondaryColor,
            c.SuccessColor,
            c.WarningColor,
            c.ErrorColor,
            c.InfoColor,
            c.NavMode);
}
