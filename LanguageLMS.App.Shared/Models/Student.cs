namespace LanguageLMS.App.Shared.Models;

public record Student(long Id, string FullName, string Email, string? Phone)
{
    public string FirstName => string.IsNullOrWhiteSpace(FullName) ? "şagird" : FullName.Trim().Split(' ')[0];
    public string Initial => FirstName.Length > 0 ? FirstName[..1].ToUpper() : "L";
}

public record AuthResult(string Token, Student User);
