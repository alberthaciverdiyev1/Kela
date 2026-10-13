using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using LanguageLMS.App.Shared.Dtos;
using LanguageLMS.App.Shared.Dtos.Auth;
using LanguageLMS.App.Shared.Dtos.Progress;
using LanguageLMS.App.Shared.Dtos.Quiz;
using LanguageLMS.App.Shared.Services;
using LanguageLMS.App.Shared.Services.Api;

namespace LanguageLMS.App.Shared.Pages.Profile;

public partial class Profile
{
    private bool _busy;
    private bool _phoneOpen;
    private bool _passwordOpen;
    private bool _privacyOpen;
    private bool _logoutOpen;
    private bool _privacyLoading;
    private bool _notifications = true;

    private string _phone = "";
    private string _currentPassword = "";
    private string _newPassword = "";
    private string? _phoneError;
    private string? _passwordError;
    private List<PrivacyPolicyDto> _privacy = new();

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

    private async Task LogoutAsync()
    {
        try { await Api.LogoutAsync(); } catch { }
        try { await Store.RemoveAsync(); } catch { }
        Auth.Clear();
    }
}
