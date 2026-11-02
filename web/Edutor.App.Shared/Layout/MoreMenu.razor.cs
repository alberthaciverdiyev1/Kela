using Microsoft.AspNetCore.Components;
using Edutor.App.Shared.Services;
using Edutor.App.Shared.Services.Api;
using Edutor.App.Shared.Services.Localization;

namespace Edutor.App.Shared.Layout;

public partial class MoreMenu
{
    [Inject] private AppApi Api { get; set; } = default!;
    [Inject] private ApiClient ApiClient { get; set; } = default!;
    [Inject] private TokenStore Store { get; set; } = default!;
    [Inject] private AuthState Auth { get; set; } = default!;

    /// <summary>Mobil: aşağıdan açılan panel; desktop: avatarın altında dropdown.</summary>
    [Parameter] public bool Sheet { get; set; }

    [Parameter] public bool Open { get; set; }

    [Parameter] public EventCallback OnClose { get; set; }

    private bool _logoutConfirm;
    private bool _busy;

    private sealed record MenuItem(string Href, string Label, string Icon, string Color);

    private MenuItem[] Items =>
    [
        new("/profile", L["Nav_Profile"], "users", "#27815e"),
        new("/notes", L["Notes_Title"], "edit", "#3b82f6"),
        new("/attendance", L["Attendance_MyTitle"], "calendar", "#f59e0b"),
        new("/payments", L["Payments_Title"], "chart", "#8b5cf6"),
        new("/results", L["Nav_Results"], "trophy", "#6366f1"),
        new("/notifications", L["Nav_Notifications"], "bell", "#ec4899"),
        new("/contact", L["Contact_Title"], "mail", "#0ea5e9")
    ];

    private bool HasPhoto => !string.IsNullOrWhiteSpace(Auth.User?.Image)
                             && !Auth.User!.Image!.Contains("noPhoto", StringComparison.OrdinalIgnoreCase);

    private string PanelCss => Sheet
        ? "fixed inset-x-0 bottom-0 z-[61] mx-auto max-h-[86vh] w-full max-w-[430px] overflow-y-auto rounded-t-[28px] bg-white px-5 pb-8 pt-2.5 shadow-[0_-10px_34px_rgba(6,78,59,0.2)] sheet-up"
        : "absolute right-0 top-[54px] z-[61] w-[330px] rounded-3xl border border-brand-104 bg-white p-4 shadow-[0_18px_44px_rgba(6,78,59,0.18)] page-fade";

    protected override void OnParametersSet()
    {
        if (!Open) _logoutConfirm = false;
    }

    private static string Tint(string hex)
        => $"rgba({Convert.ToInt32(hex[1..3], 16)},{Convert.ToInt32(hex[3..5], 16)},{Convert.ToInt32(hex[5..7], 16)},0.13)";

    private Task Close()
    {
        _logoutConfirm = false;
        return OnClose.InvokeAsync();
    }

    private async Task ChangeLanguageAsync(string code)
    {
        await L.SetAsync(code);
        StateHasChanged();
    }

    private async Task LogoutAsync()
    {
        if (_busy) return;
        _busy = true;

        try { await Api.LogoutAsync(); } catch { /* səssiz */ }

        if (ApiClient.TenantId is long tenantId)
        {
            try { await Store.RemoveAccountAsync(tenantId); } catch { /* səssiz */ }
        }

        try { await Store.RemoveAsync(); } catch { /* səssiz */ }
        Auth.Clear();

        _busy = false;
        _logoutConfirm = false;
        await OnClose.InvokeAsync();
    }
}
