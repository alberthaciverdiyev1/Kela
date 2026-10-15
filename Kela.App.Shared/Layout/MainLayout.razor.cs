using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Kela.App.Shared.Dtos;
using Kela.App.Shared.Dtos.Auth;
using Kela.App.Shared.Dtos.Progress;
using Kela.App.Shared.Dtos.Quiz;
using Kela.App.Shared.Services;
using Kela.App.Shared.Services.Api;

namespace Kela.App.Shared.Layout;

public partial class MainLayout
{
    [Inject] private NavigationManager Nav { get; set; } = default!;

    private bool _ready;
    private string _location = "/";
    private readonly List<string> _trail = [];
    private bool _goingBack;

    private string RenderKey => Lang.Current + "|" + _location;

    private string TransitionClass => _goingBack ? "page-transition page-back" : "page-transition";

    protected override async Task OnInitializedAsync()
    {
        Auth.Changed += OnAuthChanged;
        Lang.Changed += OnLangChanged;
        Nav.LocationChanged += OnLocationChanged;
        _location = Nav.ToBaseRelativePath(Nav.Uri);
        _trail.Add(_location);

        if (!Auth.IsAuthenticated)
        {
            await RestoreAsync();
        }

        await Lang.InitAsync();
        await ApplyScreenGuardAsync();
        EnforcePasswordChange();
        _ready = true;
    }

    /// <summary>
    /// Giriş vəziyyətinə görə ekran qoruması və push token qeydiyyatını tənzimləyir.
    /// Ekran qoruması bütün tələbə tətbiqində aktivdir; watermark və push token isə girişdə qurulur, çıxışda təmizlənir.
    /// </summary>
    private async Task ApplyScreenGuardAsync()
    {
        if (Auth.IsAuthenticated && Auth.User is { } user)
        {
            // Watermark-da mümkün olan bütün məlumatlar (JS tərəfi cihaz/vaxt/route-u özü əlavə edir).
            var lines = new List<string> { user.FullName, user.Email };
            if (!string.IsNullOrWhiteSpace(user.Phone)) lines.Add(user.Phone);
            lines.Add($"ID: {user.Id}");

            await ScreenGuard.EnableAsync(AppConfig.ShowWatermark ? string.Join('\n', lines) : null);
            await Push.RegisterAsync();
        }
        else
        {
            await Push.UnregisterAsync();
            await ScreenGuard.ClearWatermarkAsync();
        }
    }

    /// <summary>localStorage-dəki tokeni bərpa edir və istifadəçini /api/auth/me ilə yeniləyir.</summary>
    private async Task RestoreAsync()
    {
        try
        {
            var token = await Store.GetAsync();
            if (string.IsNullOrWhiteSpace(token)) return;

            ApiClient.TenantId = await Store.GetTenantAsync();
            ApiClient.Token = token;
            var user = await Api.MeAsync();
            Auth.Set(token, user);
        }
        catch
        {
            ApiClient.Token = null;
            try
            {
                await Store.RemoveAsync();
            }
            catch
            {
                /* JS hazır deyil */
            }
        }
    }

    private void OnAuthChanged() => _ = InvokeAsync(async () =>
    {
        await ApplyScreenGuardAsync();
        EnforcePasswordChange();
        StateHasChanged();
    });

    /// <summary>İlk girişdə (təsadüfi şifrə) profil səhifəsinə yönləndirir — şifrə dəyişmək üçün.</summary>
    private void EnforcePasswordChange()
    {
        if (Auth.User?.MustChangePassword == true &&
            !Nav.Uri.Contains("/profile", StringComparison.OrdinalIgnoreCase))
        {
            Nav.NavigateTo("/profile");
        }
    }

    private void OnLangChanged() => _ = InvokeAsync(() =>
    {
        // Dil dəyişməsi naviqasiya deyil — keçid animasiyası irəli istiqamətdə qalsın.
        _goingBack = false;
        StateHasChanged();
    });

    private void OnLocationChanged(object? sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
        => _ = InvokeAsync(() =>
        {
            var path = Nav.ToBaseRelativePath(Nav.Uri);
            _goingBack = IsBackNavigation(path);
            _location = path;
            StateHasChanged();
        });

    /// <summary>
    /// Geri naviqasiyanı müəyyən edir. Blazor istiqamət vermir, ona görə keçdiyimiz
    /// yolları yığıb hədəf yola görə qərar veririk: yol yığında varsa geri qayıdırıq.
    /// Bu, həm 'geri' düyməsini (pushState), həm history.back()-i (popstate) tutur.
    /// </summary>
    private bool IsBackNavigation(string path)
    {
        var idx = _trail.LastIndexOf(path);

        if (idx >= 0 && idx < _trail.Count - 1)
        {
            _trail.RemoveRange(idx + 1, _trail.Count - idx - 1);
            return true;
        }

        if (idx < 0) _trail.Add(path);
        return false;
    }

    public void Dispose()
    {
        Auth.Changed -= OnAuthChanged;
        Lang.Changed -= OnLangChanged;
        Nav.LocationChanged -= OnLocationChanged;
    }
}
