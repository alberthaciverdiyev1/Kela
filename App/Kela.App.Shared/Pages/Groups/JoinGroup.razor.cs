using Microsoft.AspNetCore.Components;
using Kela.App.Shared.Dtos;
using Kela.App.Shared.Services;
using Kela.App.Shared.Services.Api;

namespace Kela.App.Shared.Pages.Groups;

public partial class JoinGroup
{
    [Parameter] public string Token { get; set; } = "";

    private GroupJoinInfoDto? _info;
    private bool _loading = true;
    private bool _busy;
    private string? _error;

    private bool HasImage => !string.IsNullOrWhiteSpace(_info?.Image)
                             && !_info.Image.Contains("noPhoto", StringComparison.OrdinalIgnoreCase);

    protected override void OnInitialized() => OnRefresh(FetchAsync);

    protected override async Task OnInitializedAsync()
    {
        await FetchAsync();
        _loading = false;
    }

    private async Task FetchAsync()
    {
        _error = null;
        try { _info = await Api.JoinGroupInfoAsync(Token); }
        catch { _info = null; }
    }

    private async Task RequestAsync()
    {
        _busy = true;
        _error = null;
        try
        {
            _info = await Api.RequestJoinGroupAsync(Token);
        }
        catch (Exception ex)
        {
            _error = ex.Message;
        }
        finally
        {
            _busy = false;
        }
    }
}
