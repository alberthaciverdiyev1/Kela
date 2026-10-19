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
    [Inject] private NavIntent Intent { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private RefreshSignal Refresh { get; set; } = default!;

    private bool _ready;
    private readonly List<string> _trail = [];
    private NavKind _kind = NavKind.Forward;
    private bool _instant;
    private DateTime _lastSwap = DateTime.MinValue;

    private DotNetObjectReference<MainLayout>? _selfRef;

    // Açar yalnız route-dan asılıdır və canlı oxunur ki, @Body ilə EYNİ render-də
    // bir dəfə dəyişsin. Növ (_kind) və _instant BİLƏRƏKDƏN daxil edilmir: onlar
    // set olunanda açar ikinci dəfə dəyişər, element iki dəfə yaradılar və birinci
    // animasiya yarıda qırılardı.
    private string RenderKey => $"{Lang.Current}|{Nav.ToBaseRelativePath(Nav.Uri)}";

    private string TransitionClass => _instant || _kind == NavKind.Tab
        ? "page-transition page-none"
        : _kind == NavKind.Back
            ? "page-transition page-back"
            : "page-transition";

    protected override async Task OnInitializedAsync()
    {
        Auth.Changed += OnAuthChanged;
        Lang.Changed += OnLangChanged;
        Nav.LocationChanged += OnLocationChanged;
        _trail.Add(Nav.ToBaseRelativePath(Nav.Uri));

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
        // İlk giriş: şifrə və/və ya generasiya olunmuş ad dəyişdirilməlidir.
        var mustComplete = Auth.User?.MustChangePassword == true || Auth.User?.MustChangeName == true;
        if (mustComplete && !Nav.Uri.Contains("/profile", StringComparison.OrdinalIgnoreCase))
        {
            Nav.NavigateTo("/profile");
        }
    }

    private void OnLangChanged() => _ = InvokeAsync(StateHasChanged);

    private void OnLocationChanged(object? sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
    {
        // Növ render növbəsi işlənməmişdən əvvəl SINXRON təyin olunur.
        _kind = ResolveKind(Nav.ToBaseRelativePath(Nav.Uri), Intent.Take());

        // Ardıcıl sürətli keçidlərdə animasiya yarımçıq qırılır və "sıçrayış" kimi
        // görünür; bu halda animasiya tamamilə söndürülür.
        var now = DateTime.UtcNow;
        _instant = now - _lastSwap < TimeSpan.FromMilliseconds(300);
        _lastSwap = now;

        _ = InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Keçid növünü müəyyən edir. Əvvəlcə komponentin qeyd etdiyi niyyətə baxılır
    /// (sekme / geri); niyyət yoxdursa hədəf yolun keçdiyimiz yollarda olub-olmaması
    /// ilə geri qayıdışı təxmin edirik (hardware geri tuşu bu yolla tutulur).
    /// </summary>
    private NavKind ResolveKind(string path, NavKind intent)
    {
        switch (intent)
        {
            case NavKind.Back:
                var idx = _trail.IndexOf(path);
                if (idx >= 0) _trail.RemoveRange(idx + 1, _trail.Count - idx - 1);
                else _trail.Add(path);
                return NavKind.Back;

            case NavKind.Tab:
                if (_trail.Count == 0 || _trail[^1] != path) _trail.Add(path);
                return NavKind.Tab;

            default:
                return IsBackNavigation(path) ? NavKind.Back : NavKind.Forward;
        }
    }

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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        try
        {
            _selfRef = DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync("kelaPull.register", _selfRef);
        }
        catch
        {
            /* JS hələ hazır deyil */
        }
    }

    /// <summary>
    /// Aşağı çəkərək yeniləmə: cari səhifə ÖZ məlumatını yerində yeniləyir.
    /// Səhifə yenidən qurulmadığı üçün ekran skeletona düşmür.
    /// </summary>
    [JSInvokable]
    public async Task OnPullRefresh()
    {
        if (!Auth.IsAuthenticated) return;   // giriş ekranında yeniləmə mənasızdır
        await Refresh.RaiseAsync();
    }

    public void Dispose()
    {
        Auth.Changed -= OnAuthChanged;
        Lang.Changed -= OnLangChanged;
        Nav.LocationChanged -= OnLocationChanged;
        _selfRef?.Dispose();
    }
}
