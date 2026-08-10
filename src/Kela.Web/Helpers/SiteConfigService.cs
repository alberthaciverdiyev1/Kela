namespace Kela.Web.Helpers;

public sealed class SiteConfigService(IApiClient api)
{
    public string SiteName { get; private set; } = "Kela";
    public string NavMode { get; private set; } = "navbar";
    public string NotificationProvider { get; private set; } = "sweetalert";

    public async Task LoadAsync(CancellationToken ct = default)
    {
        try
        {
            var result = await api.GetSiteConfigAsync(ct);
            if (result.Success && result.Data is not null)
            {
                SiteName = result.Data.SiteName;
                NavMode = result.Data.NavMode;
                NotificationProvider = result.Data.NotificationProvider;
            }
        }
        catch
        {
        }
    }
}
