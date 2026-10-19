using Microsoft.AspNetCore.Components;
using Kela.App.Shared.Dtos;
using Kela.App.Shared.Services;
using Kela.App.Shared.Services.Api;

namespace Kela.App.Shared.Pages.Login;

public partial class Login
{
    private static readonly string[] CourseEmojis =
        ["🎓", "🦉", "📚", "🇹🇷", "🐍", "📊", "🟣", "🇩🇪", "🇫🇷", "📐", "⚛️", "🧪"];

    [Parameter] public string? InviteToken { get; set; }

    private bool _modalOpen;
    private string _search = "";
    private GroupJoinInfoDto? _invite;
    private string? _loadedInviteToken;

    private List<TenantDto> _tenants = new();
    private long? _tenantId;

    private string _email = "";
    private string _password = "";
    private string _fullName = "";
    private string _phone = "";
    private bool _show;
    private bool _busy;
    private bool _register;
    private string? _error;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _tenants = await Api.TenantsAsync();
            _tenantId = _tenants.FirstOrDefault()?.Id;
        }
        catch
        {
            _tenants = new List<TenantDto>();
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        if (string.IsNullOrWhiteSpace(InviteToken) || InviteToken == _loadedInviteToken) return;
        _loadedInviteToken = InviteToken;

        try
        {
            _invite = await Api.JoinGroupPublicInfoAsync(InviteToken);
            if (_invite.TenantId > 0) _tenantId = _invite.TenantId;

            // Dəvət olunan kurs siyahıda olmasa da (məs. yalnız aktivlər gəlir) seçim saxlanılır.
            if (_tenants.All(t => t.Id != _tenantId))
            {
                _tenants.Add(new TenantDto(_invite.TenantId, _invite.Name, "", true));
            }
        }
        catch
        {
            _invite = null;
        }
    }

    private string Emoji(TenantDto t) => CourseEmojis[Math.Abs(t.Id) % CourseEmojis.Length];

    private void Toggle()
    {
        _register = !_register;
        _error = null;
    }

    private void OpenModal()
    {
        _modalOpen = true;
        _search = "";
    }

    private void CloseModal() => _modalOpen = false;

    private void Select(TenantDto tenant)
    {
        _tenantId = tenant.Id;
        _error = null;
    }

    private void SelectFromModal(TenantDto tenant)
    {
        Select(tenant);
        CloseModal();
    }

    private IEnumerable<TenantDto> Filtered()
    {
        var q = _search.Trim().ToLowerInvariant();
        return q.Length == 0
            ? _tenants
            : _tenants.Where(t => t.Name.ToLowerInvariant().Contains(q));
    }

    private async Task SubmitAsync()
    {
        _busy = true;
        _error = null;
        try
        {
            if (_tenantId is not > 0)
            {
                _error = L["Login_SelectTenant"];
                return;
            }

            var user = _register
                ? await Api.RegisterAsync(_fullName.Trim(), _email.Trim(), string.IsNullOrWhiteSpace(_phone) ? null : _phone.Trim(), _password, _tenantId.Value)
                : await Api.LoginAsync(_email.Trim(), _password, _tenantId.Value);
            Auth.Set(ApiClient.Token ?? "", user);
            if (!string.IsNullOrWhiteSpace(ApiClient.Token))
            {
                await Store.SaveAsync(ApiClient.Token!);
                await Store.SaveTenantAsync(_tenantId.Value);
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
