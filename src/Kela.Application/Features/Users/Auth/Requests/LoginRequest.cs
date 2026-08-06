namespace Kela.Application.Features.Users.Auth.Requests;

/// <summary>Giriş isteği.</summary>
public sealed record LoginRequest(string Email, string Password);
