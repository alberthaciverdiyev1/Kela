using Microsoft.AspNetCore.Mvc;

namespace Kela.Core.Modules.Students.Requests;

public class CreateStudentRequest
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    /// <summary>Boş buraxılarsa təsadüfi güclü şifrə yaradılır.</summary>
    public string? Password { get; set; }

    public bool IsActive { get; set; } = true;

    [FromForm(Name = "group_id")]
    public long? GroupId { get; set; }
}

/// <summary>Yaradılan şagird. Password yalnız təsadüfi yaradılıbsa qaytarılır.</summary>
public record CreateStudentResponse(long Id, string FullName, string Email, string? Password);
