namespace Kela.Core.Modules.Tenant.Requests;

/// <summary>Public müraciət: yeni kurs + onun ilk admini. Superadmin təsdiqinə qədər aktiv olmur.</summary>
public class RegisterTenantRequest
{
    public string CourseName { get; set; } = string.Empty;

    /// <summary>Boş buraxılarsa kurs adından yaradılır.</summary>
    public string? Slug { get; set; }

    public string AdminName { get; set; } = string.Empty;

    public string AdminSurname { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string Password { get; set; } = string.Empty;

    public string ConfirmPassword { get; set; } = string.Empty;
}
