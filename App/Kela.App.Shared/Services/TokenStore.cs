using Microsoft.JSInterop;

namespace Kela.App.Shared.Services;

/// <summary>Auth tokenini brauzerin localStorage-də saxlayır — səhifə yenilənəndə sessiya qalsın.</summary>
public sealed class TokenStore(IJSRuntime js)
{
    private const string Key = "lms.token";
    private const string TenantKey = "lms.tenant";

    public ValueTask<string?> GetAsync()
        => js.InvokeAsync<string?>("localStorage.getItem", Key);

    public ValueTask SaveAsync(string token)
        => js.InvokeVoidAsync("localStorage.setItem", Key, token);

    public ValueTask RemoveAsync()
    {
        var removeToken = js.InvokeVoidAsync("localStorage.removeItem", Key).AsTask();
        var removeTenant = js.InvokeVoidAsync("localStorage.removeItem", TenantKey).AsTask();
        return new ValueTask(Task.WhenAll(removeToken, removeTenant));
    }

    public async ValueTask<long?> GetTenantAsync()
    {
        var value = await js.InvokeAsync<string?>("localStorage.getItem", TenantKey);
        return long.TryParse(value, out var id) ? id : null;
    }

    public ValueTask SaveTenantAsync(long tenantId)
        => js.InvokeVoidAsync("localStorage.setItem", TenantKey, tenantId.ToString());
}
