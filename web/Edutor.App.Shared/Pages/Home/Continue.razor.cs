using Microsoft.AspNetCore.Components;
using Edutor.App.Shared.Dtos.Progress;
using Edutor.App.Shared.Services;
using Edutor.App.Shared.Services.Api;

namespace Edutor.App.Shared.Pages.Home;

public partial class Continue
{
    private bool _loading = true;
    private string? _error;
    private List<ContinueItemDto> _items = [];

    protected override void OnInitialized() => OnRefresh(FetchAsync);

    protected override async Task OnInitializedAsync()
    {
        await FetchAsync();
        _loading = false;
    }

    private async Task FetchAsync()
    {
        try
        {
            _items = await Api.ContinueAsync();
            _error = null;
        }
        catch
        {
            _error = L["Continue_Error"];
        }
    }

    private static string IconFor(string? type) => type switch
    {
        "video" => "play",
        "voice" => "chart",
        "file" => "file-text",
        _ => "book"
    };
}
