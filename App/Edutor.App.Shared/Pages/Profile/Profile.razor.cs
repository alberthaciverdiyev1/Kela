using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Edutor.App.Shared.Dtos;
using Edutor.App.Shared.Dtos.Attendance;
using Edutor.App.Shared.Dtos.Auth;
using Edutor.App.Shared.Dtos.Progress;
using Edutor.App.Shared.Dtos.Quiz;
using Edutor.App.Shared.Services;
using Edutor.App.Shared.Services.Api;

namespace Edutor.App.Shared.Pages.Profile;

public partial class Profile
{
    protected override async Task OnInitializedAsync()
    {
        try { _attendance = await Api.MyAttendanceAsync(); }
        catch { /* profil qalan hissəsi işləməyə davam etsin */ }
        _attendanceLoading = false;

        try { _accounts = await Store.GetAccountsAsync(); } catch { /* səssiz */ }

        // İlk giriş məcburiyyətləri: şifrə və/və ya ad-soyad.
        if (Auth.User?.MustChangeName == true) OpenName();
        if (Auth.User?.MustChangePassword == true) _passwordOpen = true;
    }

    private async Task ChangeLang(ChangeEventArgs e) => await L.SetAsync(e.Value?.ToString() ?? "az");

    private bool _busy;
    private bool _phoneOpen;
    private bool _nameOpen;
    private bool _imageBusy;
    private bool _photoOpen;
    private bool _passwordOpen;
    private bool _privacyOpen;
    private bool _logoutOpen;
    private bool _privacyLoading;
    private MyAttendanceDto? _attendance;
    private bool _attendanceLoading = true;
    private bool _notifications = true;

    private string _phone = "";
    private string _fullName = "";
    private string _currentPassword = "";
    private string _newPassword = "";
    private string? _phoneError;
    private string? _nameError;
    private string? _passwordError;
    private List<PrivacyPolicyDto> _privacy = new();
    private List<SavedAccount> _accounts = new();
    private bool _accountBusy;

    private bool HasPhoto => !string.IsNullOrWhiteSpace(Auth.User?.Image)
                              && !Auth.User!.Image!.Contains("noPhoto", StringComparison.OrdinalIgnoreCase);

    private void OpenName()
    {
        _fullName = Auth.User?.FullName ?? "";
        _nameError = null;
        _nameOpen = true;
    }

    private async Task SaveNameAsync()
    {
        _busy = true;
        _nameError = null;
        try
        {
            var user = await Api.ChangeFullNameAsync(_fullName.Trim());
            Auth.SetUser(user);
            _nameOpen = false;
        }
        catch (Exception ex) { _nameError = ex.Message; }
        finally { _busy = false; }
    }

    private async Task OnImageSelected(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file is null) return;

        _imageBusy = true;
        try
        {
            await using var stream = file.OpenReadStream(maxAllowedSize: 5_000_000);
            var user = await Api.ChangeImageAsync(stream, file.Name, file.ContentType);
            Auth.SetUser(user);
        }
        catch { /* səssiz */ }
        finally { _imageBusy = false; }
    }

    private void OpenPhone()
    {
        _phone = Auth.User?.Phone ?? "";
        _phoneError = null;
        _phoneOpen = true;
    }

    private async Task SavePhoneAsync()
    {
        _busy = true;
        _phoneError = null;
        try
        {
            var user = await Api.ChangePhoneAsync(_phone.Trim());
            Auth.SetUser(user);
            _phoneOpen = false;
        }
        catch (Exception ex) { _phoneError = ex.Message; }
        finally { _busy = false; }
    }

    private async Task SavePasswordAsync()
    {
        _busy = true;
        _passwordError = null;
        try
        {
            await Api.ChangePasswordAsync(_currentPassword, _newPassword);
            _currentPassword = _newPassword = "";
            _passwordOpen = false;
            try { Auth.SetUser(await Api.MeAsync()); } catch { }
        }
        catch (Exception ex) { _passwordError = ex.Message; }
        finally { _busy = false; }
    }

    private async Task ToggleNotificationsAsync()
    {
        var next = !_notifications;
        try
        {
            await Api.NotificationStatusAsync(next);
            _notifications = next;
        }
        catch { /* səssiz */ }
    }

    private async Task OpenPrivacyAsync()
    {
        _privacyOpen = true;
        if (_privacy.Count > 0) return;

        _privacyLoading = true;
        try { _privacy = await Api.PrivacyPolicyAsync(); }
        catch { }
        _privacyLoading = false;
    }

    private async Task SwitchAccountAsync(SavedAccount account)
    {
        if (_accountBusy || account.TenantId == ApiClient.TenantId) return;

        _accountBusy = true;
        var previousTenant = ApiClient.TenantId;
        var previousToken = ApiClient.Token;

        try
        {
            ApiClient.TenantId = account.TenantId;
            ApiClient.Token = account.Token;

            var user = await Api.MeAsync();
            Auth.Set(account.Token, user);
            await Store.SaveAsync(account.Token);
            await Store.SaveTenantAsync(account.TenantId);
            _accounts = await Store.GetAccountsAsync();
        }
        catch
        {
            // Token vaxtı bitib — hesabı siyahıdan çıxar və əvvəlki sessiyaya qaytar.
            ApiClient.TenantId = previousTenant;
            ApiClient.Token = previousToken;
            await Store.RemoveAccountAsync(account.TenantId);
            _accounts = await Store.GetAccountsAsync();
        }
        finally
        {
            _accountBusy = false;
        }
    }

    private async Task LogoutAsync()
    {
        try { await Api.LogoutAsync(); } catch { }

        if (ApiClient.TenantId is long tenantId)
        {
            try { await Store.RemoveAccountAsync(tenantId); } catch { /* səssiz */ }
        }

        try { await Store.RemoveAsync(); } catch { }
        Auth.Clear();
    }
}
