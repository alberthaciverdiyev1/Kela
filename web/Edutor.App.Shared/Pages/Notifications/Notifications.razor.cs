using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Edutor.App.Shared.Dtos.Notification;
using Edutor.App.Shared.Services.Api;

namespace Edutor.App.Shared.Pages.Notifications;

public partial class Notifications
{
    private const int PageSize = 20;

    private bool _loading = true;
    private bool _busy;
    private bool _pushBusy;
    private string _pushState = "hidden"; // hidden | prompt | blocked
    private bool _deniedModalOpen;
    private string _webPushKey = string.Empty;
    private int _page;
    private int _lastPage = 1;
    private readonly List<NotificationDto> _items = new();

    // Yeniləmə: siyahını başa qaytarıb ilk səhifəni yenidən çəkir.
    protected override void OnInitialized() => OnRefresh(async () =>
    {
        _page = 0;
        _items.Clear();
        await FetchAsync();
    });

    protected override async Task OnInitializedAsync()
    {
        await CheckPushAsync();
        await FetchAsync();
    }

    private async Task CheckPushAsync()
    {
        try
        {
            if (!await JS.InvokeAsync<bool>("edutorPush.supported")) return;

            var permission = await JS.InvokeAsync<string>("edutorPush.permission");
            if (permission == "denied")
            {
                // Hər girişdə xatırladılır: bildirişlər profildən söndürülə bilər.
                _pushState = "blocked";
                _deniedModalOpen = true;
                return;
            }

            // Açar əvvəlcədən yüklənir ki, "aktivləşdir" klikində JS sorğusu jest
            // daxilində — heç bir HTTP gözləməsi olmadan — başlasın.
            _webPushKey = await Api.WebPushKeyAsync();

            // İcazə verilib, amma abunəlik yoxdursa (brauzer yaddaşı təmizlənib) yenidən abunə olunmalıdır.
            if (permission == "granted")
            {
                var subscription = await JS.InvokeAsync<string?>("edutorPush.current");
                _pushState = string.IsNullOrWhiteSpace(subscription) ? "prompt" : "hidden";
                return;
            }

            _pushState = "prompt";
        }
        catch
        {
            /* JS hazır deyil (native host) */
        }
    }

    private async Task EnablePushAsync()
    {
        if (_pushBusy) return;
        _pushBusy = true;

        try
        {
            if (string.IsNullOrWhiteSpace(_webPushKey))
            {
                _pushState = "hidden";
                return;
            }

            // İlk await JS sorğusudur: brauzer icazə pəncərəsini yalnız klik jesti
            // daxilində göstərir; əvvəlcədən HTTP gözləməsi jesti itirir.
            var subscription = await JS.InvokeAsync<string?>("edutorPush.request", _webPushKey);
            if (string.IsNullOrWhiteSpace(subscription))
            {
                if (await JS.InvokeAsync<string>("edutorPush.permission") == "denied")
                {
                    _pushState = "blocked";
                    _deniedModalOpen = true;
                }
                else
                {
                    _pushState = "prompt";
                }
                return;
            }

            await Api.SaveNotificationTokenAsync(subscription, "web");
            _pushState = "hidden";
        }
        catch
        {
            /* sükutla — push qeydiyyatı kritik deyil */
        }
        finally
        {
            _pushBusy = false;
        }
    }

    /// <summary>Növbəti səhifəni çəkir (ilk səhifədə siyahını sıfırlayır).</summary>
    /// <summary>Bildirişin Data sahəsindəki təhlükəsiz daxili route (yalnız /homeworks/...).</summary>
    private static string? HomeworkRoute(NotificationDto n)
    {
        if (string.IsNullOrWhiteSpace(n.Data)) return null;

        try
        {
            using var document = JsonDocument.Parse(n.Data);
            if (!document.RootElement.TryGetProperty("route", out var route)) return null;

            var value = route.GetString();
            return value is not null && value.StartsWith("/homeworks/", StringComparison.Ordinal) ? value : null;
        }
        catch (JsonException) { return null; }
    }

    private async Task FetchAsync()
    {
        if (_busy) return;
        _busy = true;

        try
        {
            var next = _page + 1;
            var result = await Api.NotificationsAsync(next, PageSize);

            if (next == 1) _items.Clear();
            _items.AddRange(result.Data);
            _page = result.CurrentPage;
            _lastPage = result.LastPage;
        }
        catch
        {
            /* sükutla */
        }
        finally
        {
            _busy = false;
            _loading = false;
        }
    }
}
