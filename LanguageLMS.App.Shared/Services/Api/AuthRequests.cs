using LanguageLMS.App.Shared.Dtos;
using LanguageLMS.App.Shared.Dtos.Auth;
using LanguageLMS.App.Shared.Dtos.Progress;
using LanguageLMS.App.Shared.Dtos.Quiz;

namespace LanguageLMS.App.Shared.Services.Api;

public sealed partial class AppApi
{
    // --- Auth ---
    public async Task<StudentDto> LoginAsync(string email, string password)
    {
        var result = await _api.PostAsync<AuthResultDto>("/api/auth/login", new { email, password });
        _api.Token = result.Token;
        return result.User;
    }

    public async Task<StudentDto> RegisterAsync(string fullName, string email, string? phone, string password)
    {
        var result = await _api.PostAsync<AuthResultDto>("/api/auth/register",
            new { fullName, email, phone, password });
        _api.Token = result.Token;
        return result.User;
    }

    public Task<StudentDto> MeAsync() => _api.GetAsync<StudentDto>("/api/auth/me");
    public Task LogoutAsync() => _api.PostAsync<object>("/api/auth/logout");
}
