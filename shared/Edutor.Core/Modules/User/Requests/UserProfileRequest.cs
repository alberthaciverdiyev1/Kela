namespace Edutor.Core.Modules.User.Requests;

/// <summary>İstifadəçinin öz profilini redaktə etməsi (rol dəyişmir).</summary>
public class UserProfileRequest
{
    public string? Name { get; set; }

    public string? Surname { get; set; }

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }
}

public class UserChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;

    public string NewPassword { get; set; } = string.Empty;
}
