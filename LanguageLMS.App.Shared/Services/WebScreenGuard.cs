using Microsoft.JSInterop;
using LanguageLMS.App.Shared.Services.Api;

namespace LanguageLMS.App.Shared.Services;

public sealed class WebScreenGuard(IJSRuntime js, AppApi api) : IScreenGuard, IAsyncDisposable
{
    private static readonly TimeSpan Throttle = TimeSpan.FromSeconds(5);

    private DotNetObjectReference<WebScreenGuard>? _ref;
    private readonly Dictionary<string, DateTimeOffset> _lastSent = new();

    public async ValueTask EnableAsync(string? watermark = null)
    {
        try
        {
            _ref ??= DotNetObjectReference.Create(this);
            NativeScreenGuard.Apply();
            await js.InvokeVoidAsync("lmsGuard.registerCallback", _ref);
            await js.InvokeVoidAsync("lmsGuard.enable");
            if (!string.IsNullOrWhiteSpace(watermark))
            {
                await js.InvokeVoidAsync("lmsGuard.setWatermark", watermark);
            }
        }
        catch
        {
            /* JS hələ hazır deyil — sükutla */
        }
    }

    public async ValueTask ClearWatermarkAsync()
    {
        try { await js.InvokeVoidAsync("lmsGuard.clearWatermark"); }
        catch { /* sükutla */ }
    }

    public async ValueTask DisableAsync()
    {
        try
        {
            NativeScreenGuard.Remove();
            if (_ref is not null) await js.InvokeVoidAsync("lmsGuard.unregisterCallback", _ref);
            await js.InvokeVoidAsync("lmsGuard.disable");
        }
        catch
        {
            /* sükutla */
        }
    }

    /// <summary>JS tərəfdən gələn pozuntu (throttle olunur, sonra serverə yazılır).</summary>
    [JSInvokable]
    public async Task OnViolation(string type, string? path)
    {
        if (string.IsNullOrWhiteSpace(type)) return;

        var now = DateTimeOffset.UtcNow;
        if (_lastSent.TryGetValue(type, out var last) && now - last < Throttle) return;
        _lastSent[type] = now;

        try
        {
            await api.ReportScreenViolationAsync(type, path);
        }
        catch
        {
            /* şəbəkə xətası — qorumaya təsir etməsin */
        }
    }

    public async ValueTask DisposeAsync()
    {
        try { await js.InvokeVoidAsync("lmsGuard.unregisterCallback", _ref); } catch { /* sükutla */ }
        _ref?.Dispose();
    }
}
