namespace Edutor.Core.Modules.Students.Requests;

public class ResetStudentPasswordRequest
{
    /// <summary>Boş buraxılarsa təsadüfi güclü şifrə yaradılır.</summary>
    public string? Password { get; set; }
}

/// <summary>Password yalnız təsadüfi yaradılıbsa qaytarılır.</summary>
public record ResetStudentPasswordResponse(long Id, string Email, string? Password);
