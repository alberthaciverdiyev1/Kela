using Microsoft.AspNetCore.Components;
using Edutor.App.Shared.Dtos;
using Edutor.App.Shared.Services;
using Edutor.App.Shared.Services.Api;

namespace Edutor.App.Shared.Pages.Groups;

public partial class JoinGroup
{
    [Parameter] public string Token { get; set; } = "";

    private GroupJoinInfoDto? _info;
    private GroupJoinInfoDto? _otherTenant;
    private SavedAccount? _savedAccount;
    private bool _loading = true;
    private bool _busy;
    private bool _switchBusy;
    private string? _error;
    private string? _switchError;

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
        _info = null;
        _otherTenant = null;
        _savedAccount = null;

        try
        {
            _info = await Api.JoinGroupInfoAsync(Token);
            return;
        }
        catch
        {
            // Cari tenant-da tapılmadı — bəlkə link başqa kursa aiddir.
        }

        try
        {
            var publicInfo = await Api.JoinGroupPublicInfoAsync(Token);
            if (publicInfo.TenantId > 0 && publicInfo.TenantId != ApiClient.TenantId)
            {
                _otherTenant = publicInfo;

                var accounts = await Store.GetAccountsAsync();
                _savedAccount = accounts.FirstOrDefault(a => a.TenantId == publicInfo.TenantId);
            }
        }
        catch
        {
            _otherTenant = null;
        }
    }

    /// <summary>Yadda saxlanılan hesabla dəvət olunan tenant-a keçid.</summary>
    private async Task SwitchSavedAccountAsync()
    {
        if (_savedAccount is null || _switchBusy) return;

        _switchBusy = true;
        _switchError = null;
        var previousTenant = ApiClient.TenantId;
        var previousToken = ApiClient.Token;

        try
        {
            ApiClient.TenantId = _savedAccount.TenantId;
            ApiClient.Token = _savedAccount.Token;

            var user = await Api.MeAsync();
            Auth.Set(_savedAccount.Token, user);
            await Store.SaveAsync(_savedAccount.Token);
            await Store.SaveTenantAsync(_savedAccount.TenantId);

            await FetchAsync();
        }
        catch
        {
            ApiClient.TenantId = previousTenant;
            ApiClient.Token = previousToken;
            await Store.RemoveAccountAsync(_savedAccount.TenantId);
            _savedAccount = null;
            _switchError = L["Groups_JoinSwitchFailed"];
        }
        finally
        {
            _switchBusy = false;
        }
    }

    /// <summary>Cari sessiyanı bağlayır — dəvət linki kontekstində login/qeydiyyat açılır.</summary>
    private async Task SignInOtherAsync()
    {
        if (_switchBusy) return;

        _switchBusy = true;
        try
        {
            try { await Api.LogoutAsync(); } catch { /* səssiz */ }
            try { await Store.RemoveAsync(); } catch { /* səssiz */ }
            Auth.Clear();
            Nav.NavigateTo($"/join/{Uri.EscapeDataString(Token)}", forceLoad: false);
        }
        finally
        {
            _switchBusy = false;
        }
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
