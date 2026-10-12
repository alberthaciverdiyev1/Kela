using Microsoft.JSInterop;

namespace LanguageLMS.App.Shared.Services;

/// <summary>Auth tokenini brauzerin localStorage-də saxlayır — səhifə yenilənəndə sessiya qalsın.</summary>
public sealed class TokenStore(IJSRuntime js)
{
    private const string Key = "lms.token";

    public ValueTask<string?> GetAsync()
        => js.InvokeAsync<string?>("localStorage.getItem", Key);

    public ValueTask SaveAsync(string token)
        => js.InvokeVoidAsync("localStorage.setItem", Key, token);

    public ValueTask RemoveAsync()
        => js.InvokeVoidAsync("localStorage.removeItem", Key);
}
