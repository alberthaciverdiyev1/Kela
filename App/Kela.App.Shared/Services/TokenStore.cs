using System.Text.Json;
using Microsoft.JSInterop;

namespace Kela.App.Shared.Services;

/// <summary>Auth tokenini brauzerin localStorage-də saxlayır — səhifə yenilənəndə sessiya qalsın.</summary>
public sealed class TokenStore(IJSRuntime js)
{
    private const string Key = "lms.token";
    private const string TenantKey = "lms.tenant";
    private const string AccountsKey = "lms.accounts";

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

    /// <summary>Yadda saxlanılan tenant hesabları (profil səhifəsində keçid üçün).</summary>
    public async ValueTask<List<SavedAccount>> GetAccountsAsync()
    {
        try
        {
            var json = await js.InvokeAsync<string?>("localStorage.getItem", AccountsKey);
            return string.IsNullOrWhiteSpace(json)
                ? new List<SavedAccount>()
                : JsonSerializer.Deserialize<List<SavedAccount>>(json) ?? new List<SavedAccount>();
        }
        catch
        {
            return new List<SavedAccount>();
        }
    }

    public async ValueTask SaveAccountAsync(SavedAccount account)
    {
        var accounts = await GetAccountsAsync();
        accounts.RemoveAll(a => a.TenantId == account.TenantId);
        accounts.Add(account);
        await js.InvokeVoidAsync("localStorage.setItem", AccountsKey, JsonSerializer.Serialize(accounts));
    }

    public async ValueTask RemoveAccountAsync(long tenantId)
    {
        var accounts = await GetAccountsAsync();
        accounts.RemoveAll(a => a.TenantId == tenantId);
        await js.InvokeVoidAsync("localStorage.setItem", AccountsKey, JsonSerializer.Serialize(accounts));
    }
}
