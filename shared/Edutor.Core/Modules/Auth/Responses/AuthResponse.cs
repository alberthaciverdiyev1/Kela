namespace Edutor.Core.Modules.Auth.Responses;

public record StudentResponse(long Id, string FullName, string Email, string? Phone, string? Image, bool MustChangePassword = false, bool MustChangeName = false, bool NotificationsEnabled = true);

public record AuthResponse(string Token, StudentResponse User);
