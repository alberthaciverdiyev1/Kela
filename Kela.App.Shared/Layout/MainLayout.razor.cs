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
    private bool _ready;

    protected override async Task OnInitializedAsync()
    {
        Auth.Changed += OnAuthChanged;
        Lang.Changed += OnLangChanged;

        if (!Auth.IsAuthenticated)
        {
            await RestoreAsync();
        }

        await Lang.InitAsync();
        await ApplyScreenGuardAsync();
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
        StateHasChanged();
    });

    private void OnLangChanged() => _ = InvokeAsync(StateHasChanged);

    public void Dispose()
    {
        Auth.Changed -= OnAuthChanged;
        Lang.Changed -= OnLangChanged;
    }
}
