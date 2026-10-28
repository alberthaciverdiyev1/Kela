namespace Edutor.App.Shared.Services;

/// <summary>Yadda saxlanılan tenant hesabı — profil səhifəsində tenantlar arası keçid üçün.</summary>
public sealed record SavedAccount(long TenantId, string TenantName, string Email, string Token);
