namespace Kela.Web.Models.Auth;

public sealed class RegisterViewModel
{
    public string FirstName { get; set; } = "";

    public string? LastName { get; set; }

    public string Email { get; set; } = "";

    public string Password { get; set; } = "";

    public string ConfirmPassword { get; set; } = "";
}
