using Kela.Application.Features.SiteConfiguration.Requests;
using Kela.Application.Features.SiteConfiguration.Responses;

namespace Kela.Application.Features.SiteConfiguration;

/// <summary>
/// Site geneli konfigürasyon servisi — TEK entity, TEK request, TEK response.
/// Tüm site ayarları tek GET ile okunur, tek PUT ile güncellenir.
/// </summary>
public interface ISiteConfigurationService
{
    /// <summary>Site konfigürasyonunu döndürür (satır yoksa varsayılanlarla oluşturur).</summary>
    Task<SiteConfigurationResponse> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>Site konfigürasyonunu günceller (Admin/Teacher).</summary>
    Task UpdateAsync(UpdateSiteConfigurationRequest request, CancellationToken cancellationToken = default);
}
