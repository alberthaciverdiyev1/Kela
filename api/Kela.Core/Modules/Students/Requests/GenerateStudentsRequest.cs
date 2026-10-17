namespace Kela.Core.Modules.Students.Requests;

public class GenerateStudentsRequest
{
    public int Count { get; set; } = 10;

    public string EmailPrefix { get; set; } = "student";

    /// <summary>E-poçt domeni (default: kela domeni).</summary>
    public string EmailDomain { get; set; } = "kela.az";

    /// <summary>Boş buraxılarsa, hər şagirdə təsadüfi güclü şifrə yaradılır.</summary>
    public string? Password { get; set; }

    public string FullNamePrefix { get; set; } = "Student";

    /// <summary>Verilərsə, yaradılan şagirdlər bu qrupa əlavə olunur.</summary>
    public long? GroupId { get; set; }
}
