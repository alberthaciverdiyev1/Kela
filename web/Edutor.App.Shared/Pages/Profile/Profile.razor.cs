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
        try { _accounts = await Store.GetAccountsAsync(); } catch { /* səssiz */ }
        _notifications = Auth.User?.NotificationsEnabled ?? true;

        // İlk giriş məcburiyyətləri: şifrə və/və ya ad-soyad.
        if (Auth.User?.MustChangeName == true) OpenName();
        if (Auth.User?.MustChangePassword == true) _passwordOpen = true;
    }

    private bool _busy;
    private bool _phoneOpen;
    private bool _nameOpen;
    private bool _imageBusy;
    private bool _photoOpen;
    private bool _passwordOpen;
    private bool _privacyOpen;
    private bool _privacyLoading;
    private bool _notifications = true;
    private bool _notifBusy;

    private string _phone = "";
    private string _fullName = "";
    private string _currentPassword = "";
    private string _newPassword = "";
    private string? _phoneError;
    private string? _nameError;
    private string? _passwordError;
    private string? _imageError;
    private string? _imageSaved;
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

        _imageError = null;
        _imageSaved = null;
        _imageBusy = true;

        try
        {
            // Orijinal şəkil yaddaşa alınır və kırpma brauzerdə aparılır; yüklənən
            // nəticə 512×512 JPEG-dir — böyük telefon fotoları da limitsiz keçir.
            using var source = new MemoryStream();
            await using (var read = file.OpenReadStream(maxAllowedSize: 20_000_000))
            {
                await read.CopyToAsync(source);
            }

            var contentType = string.IsNullOrWhiteSpace(file.ContentType) ? "image/jpeg" : file.ContentType;
            var dataUrl = $"data:{contentType};base64,{Convert.ToBase64String(source.ToArray())}";

            CropResult? cropped = null;
            var cropUnavailable = false;

            try
            {
                cropped = await JS.InvokeAsync<CropResult?>("edutorPhotoCrop.open", dataUrl, new
                {
                    title = L["PhotoCrop_Title"],
                    hint = L["PhotoCrop_Hint"],
                    zoom = L["PhotoCrop_Zoom"],
                    cancel = L["Common_Cancel"],
                    save = L["Common_Save"]
                });
            }
            catch
            {
                // Kırpma modulu yüklənməyibsə şəkil olduğu kimi göndərilir.
                cropUnavailable = true;
            }

            if (cropped is null && !cropUnavailable) return; // istifadəçi ləğv etdi

            byte[] bytes;
            string fileName, uploadType;

            if (cropped is not null)
            {
                var comma = cropped.DataUrl.IndexOf(',');
                bytes = Convert.FromBase64String(comma >= 0 ? cropped.DataUrl[(comma + 1)..] : cropped.DataUrl);
                fileName = "avatar.jpg";
                uploadType = cropped.ContentType;
            }
            else
            {
                bytes = source.ToArray();
                fileName = file.Name;
                uploadType = contentType;
            }

            using var upload = new MemoryStream(bytes);
            var user = await Api.ChangeImageAsync(upload, fileName, uploadType);
            Auth.SetUser(user);
            _imageSaved = L["Profile_ImageUpdated"];
        }
        catch
        {
            _imageError = L["Profile_ImageFailed"];
        }
        finally { _imageBusy = false; }
    }

    private sealed record CropResult(string DataUrl, string ContentType);

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
        if (_notifBusy) return;
        _notifBusy = true;
        try
        {
            var user = await Api.NotificationStatusAsync(!_notifications);
            Auth.SetUser(user);
            _notifications = user.NotificationsEnabled;

            // Yenidən aktivləşdirildikdə cihaz tokeni serverdə bərpa olunur.
            if (_notifications) await Push.RegisterAsync();
        }
        catch { /* səssiz */ }
        finally { _notifBusy = false; }
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

}
