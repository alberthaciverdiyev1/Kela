using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using LanguageLMS.App.Shared.Dtos;
using LanguageLMS.App.Shared.Dtos.Auth;
using LanguageLMS.App.Shared.Dtos.Progress;
using LanguageLMS.App.Shared.Dtos.Quiz;
using LanguageLMS.App.Shared.Services;
using LanguageLMS.App.Shared.Services.Api;

namespace LanguageLMS.App.Shared.Layout;

public partial class MainLayout
{
    private bool _ready;

    protected override async Task OnInitializedAsync()
    {
        Auth.Changed += OnAuthChanged;

        if (!Auth.IsAuthenticated)
        {
            await RestoreAsync();
        }

        _ready = true;
    }

    /// <summary>localStorage-dəki tokeni bərpa edir və istifadəçini /api/auth/me ilə yeniləyir.</summary>
    private async Task RestoreAsync()
    {
        try
        {
            var token = await Store.GetAsync();
            if (string.IsNullOrWhiteSpace(token)) return;

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

    private void OnAuthChanged() => InvokeAsync(StateHasChanged);

    public void Dispose() => Auth.Changed -= OnAuthChanged;
}
