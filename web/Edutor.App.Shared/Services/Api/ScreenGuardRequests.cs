namespace Edutor.App.Shared.Services.Api;

public sealed partial class AppApi
{
    /// <summary>Ekran qoruması pozuntusunu serverə bildirir (screen_violations).</summary>
    public Task<string> ReportScreenViolationAsync(string type, string? path)
        => _api.PostAsync<string>("/api/screen-guard/violations", new { type, path });
}
