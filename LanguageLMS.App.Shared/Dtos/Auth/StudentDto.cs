namespace LanguageLMS.App.Shared.Dtos.Auth;

public record StudentDto(long Id, string FullName, string Email, string? Phone, string? Image)
{
    public string FirstName => string.IsNullOrWhiteSpace(FullName) ? "şagird" : FullName.Trim().Split(' ')[0];
    public string Initial => FirstName.Length > 0 ? FirstName[..1].ToUpper() : "L";
}
