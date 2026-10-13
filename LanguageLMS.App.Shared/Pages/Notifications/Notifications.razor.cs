using Microsoft.AspNetCore.Components;
using LanguageLMS.App.Shared.Dtos.Notification;
using LanguageLMS.App.Shared.Services.Api;

namespace LanguageLMS.App.Shared.Pages.Notifications;

public partial class Notifications
{
    private const int PageSize = 20;

    private bool _loading = true;
    private bool _busy;
    private int _page;
    private int _lastPage = 1;
    private readonly List<NotificationDto> _items = new();

    protected override Task OnInitializedAsync() => LoadAsync();

    /// <summary>Növbəti səhifəni çəkir (ilk səhifədə siyahını sıfırlayır).</summary>
    private async Task LoadAsync()
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
