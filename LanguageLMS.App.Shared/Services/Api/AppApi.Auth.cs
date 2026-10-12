using LanguageLMS.App.Shared.Models;

namespace LanguageLMS.App.Shared.Services;

public sealed partial class AppApi
{
    // --- Auth ---
    public async Task<Student> LoginAsync(string email, string password)
    {
        var result = await _api.PostAsync<AuthResult>("/api/auth/login", new { email, password });
        _api.Token = result.Token;
        return result.User;
    }

    public async Task<Student> RegisterAsync(string fullName, string email, string? phone, string password)
    {
        var result = await _api.PostAsync<AuthResult>("/api/auth/register",
            new { fullName, email, phone, password });
        _api.Token = result.Token;
        return result.User;
    }

    public Task<Student> MeAsync() => _api.GetAsync<Student>("/api/auth/me");
    public Task LogoutAsync() => _api.PostAsync<object>("/api/auth/logout");
}
