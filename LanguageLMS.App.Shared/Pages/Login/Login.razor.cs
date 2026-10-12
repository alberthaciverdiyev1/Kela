using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using LanguageLMS.App.Shared.Dtos;
using LanguageLMS.App.Shared.Dtos.Auth;
using LanguageLMS.App.Shared.Dtos.Progress;
using LanguageLMS.App.Shared.Dtos.Quiz;
using LanguageLMS.App.Shared.Services;
using LanguageLMS.App.Shared.Services.Api;

namespace LanguageLMS.App.Shared.Pages.Login;

public partial class Login
{
    private string _email = "";
    private string _password = "";
    private string _fullName = "";
    private string _phone = "";
    private bool _show;
    private bool _busy;
    private bool _register;
    private string? _error;

    private void Toggle()
    {
        _register = !_register;
        _error = null;
    }

    private async Task SubmitAsync()
    {
        _busy = true;
        _error = null;
        try
        {
            var user = _register
                ? await Api.RegisterAsync(_fullName.Trim(), _email.Trim(), string.IsNullOrWhiteSpace(_phone) ? null : _phone.Trim(), _password)
                : await Api.LoginAsync(_email.Trim(), _password);
            Auth.Set(ApiClient.Token ?? "", user);
            if (!string.IsNullOrWhiteSpace(ApiClient.Token))
            {
                await Store.SaveAsync(ApiClient.Token!);
            }
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
