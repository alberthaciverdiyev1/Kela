namespace Kela.Core.Modules.Auth.Responses;

public record StudentResponse(long Id, string FullName, string Email, string? Phone, string? Image);

public record AuthResponse(string Token, StudentResponse User);
