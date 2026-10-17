namespace Kela.Core.Modules.User.Responses;

public record UserResponse(long Id, string? Name, string? Surname, string Email, string? PhoneNumber, string Type);
