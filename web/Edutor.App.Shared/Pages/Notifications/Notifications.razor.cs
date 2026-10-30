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
                _pushState = "blocked";
                return;
            }

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
            var key = await Api.WebPushKeyAsync();
            if (string.IsNullOrWhiteSpace(key))
            {
                _pushState = "hidden";
                return;
            }

            var subscription = await JS.InvokeAsync<string?>("edutorPush.subscribe", key);
            if (string.IsNullOrWhiteSpace(subscription))
            {
                _pushState = await JS.InvokeAsync<string>("edutorPush.permission") == "denied" ? "blocked" : "prompt";
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
