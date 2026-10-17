namespace Kela.Core.Modules.Auth.Requests;

public class SendOtpRequest
{
    public string Email { get; set; } = string.Empty;
}

public class CheckOtpRequest
{
    public string Email { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    public string Email { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ChangeEmailRequest
{
    public string Email { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
}

public class ChangePhoneRequest
{
    public string Phone { get; set; } = string.Empty;
}

public class ChangeFullNameRequest
{
    public string FullName { get; set; } = string.Empty;
}

public class NotificationStatusRequest
{
    public bool Enabled { get; set; }
}
