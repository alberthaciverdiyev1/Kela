using Microsoft.AspNetCore.Http;

namespace Kela.Core.Modules.Tenant.Requests;

/// <summary>Public müraciət: yeni kurs + onun ilk admini. Superadmin təsdiqinə qədər aktiv olmur.</summary>
public class RegisterTenantRequest
{
    /// <summary>course = kurs (admin hesabı); teacher = fərdi müəllim (müəllim hesabı).</summary>
    public string AccountType { get; set; } = "course";

    public string CourseName { get; set; } = string.Empty;

    /// <summary>İstəyə bağlı kurs loqosu.</summary>
    public IFormFile? Logo { get; set; }

    public string AdminName { get; set; } = string.Empty;

    public string AdminSurname { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>İstifadə şərtləri və məxfilik siyasəti qəbul edilib.</summary>
    public bool AcceptTerms { get; set; }

    /// <summary>Honeypot — istifadəçi görmür; botlar doldurur.</summary>
    public string? Website { get; set; }
}
