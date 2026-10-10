namespace LanguageLMS.Core.Modules.User.Requests;

public class UserUpsertRequest
{
    public string? Name { get; set; }

    public string? Surname { get; set; }

    public string Email { get; set; } = string.Empty;

    /// <summary>Oluşturmada zorunlu; güncellemede boş bırakılırsa mevcut şifre korunur.</summary>
    public string? Password { get; set; }

    public string? PhoneNumber { get; set; }

    public string Type { get; set; } = "teacher";
}
