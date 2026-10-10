namespace LanguageLMS.App.Infrastructure.Api.Auth;

public sealed record LoginRequest(string Email, string Password);

public sealed record UserProfile(long Id, string? Name, string? Surname, string Email, string? PhoneNumber, string Type);

public sealed record LoginResponse(string Token, UserProfile User);
