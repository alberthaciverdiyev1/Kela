namespace Kela.Core.Modules.Students.Requests;

public class GenerateStudentsRequest
{
    public int Count { get; set; } = 10;

    public string EmailPrefix { get; set; } = "student";

    public string EmailDomain { get; set; } = "generated.local";

    public string Password { get; set; } = "student123";

    public string FullNamePrefix { get; set; } = "Student";

    /// <summary>Verilərsə, yaradılan şagirdlər bu qrupa əlavə olunur.</summary>
    public long? GroupId { get; set; }

    /// <summary>Yaradılan şagirdlərə email + şifrə göndərilsin.</summary>
    public bool SendCredentials { get; set; }
}
