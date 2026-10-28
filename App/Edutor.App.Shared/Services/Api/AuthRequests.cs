using Edutor.App.Shared.Dtos;
using Edutor.App.Shared.Dtos.Auth;
using Edutor.App.Shared.Dtos.Progress;
using Edutor.App.Shared.Dtos.Quiz;

namespace Edutor.App.Shared.Services.Api;

public sealed partial class AppApi
{
    // --- Auth ---
    public async Task<StudentDto> LoginAsync(string email, string password, long tenantId)
    {
        _api.TenantId = tenantId;
        var result = await _api.PostAsync<AuthResultDto>("/api/auth/login", new { email, password, tenantId });
        _api.Token = result.Token;
        return result.User;
    }

    public async Task<StudentDto> RegisterAsync(string fullName, string email, string? phone, string password, long tenantId)
    {
        _api.TenantId = tenantId;
        var result = await _api.PostAsync<AuthResultDto>("/api/auth/register",
            new { fullName, email, phone, password, tenantId });
        _api.Token = result.Token;
        return result.User;
    }

    public Task<StudentDto> MeAsync() => _api.GetAsync<StudentDto>("/api/auth/me");
    public Task LogoutAsync() => _api.PostAsync<object>("/api/auth/logout");

    public Task<StudentDto> ChangePhoneAsync(string phone)
        => _api.PutAsync<StudentDto>("/api/auth/change-phone", new { phone });

    public Task ChangePasswordAsync(string currentPassword, string newPassword)
        => _api.PostAsync<object>("/api/auth/change-password", new { currentPassword, newPassword });

    public Task<StudentDto> NotificationStatusAsync(bool enabled)
        => _api.PutAsync<StudentDto>("/api/auth/notification-status", new { enabled });

    public Task<StudentDto> ChangeFullNameAsync(string fullName)
        => _api.PutAsync<StudentDto>("/api/auth/change-full-name", new { fullName });

    public Task<StudentDto> ChangeImageAsync(Stream content, string fileName, string contentType)
        => _api.PostFileAsync<StudentDto>("/api/auth/change-image", content, fileName, contentType);
}
