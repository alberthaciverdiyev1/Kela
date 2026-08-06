namespace Kela.Domain.Common;

/// <summary>
/// Identity rol adları (AspNetRoles tablosundaki string değerler).
/// Yetki veren TEK kaynak bunlardır — enum ordinal'i değil.
/// Yeni rol eklemek için: buraya sabit ekle + Program.cs seed'e + gerekli
/// authorization'a. API response'ları da bu string değerleri döndürür.
/// </summary>
public static class RoleNames
{
    public const string Admin   = "Admin";
    public const string Teacher = "Teacher";
    public const string Student = "Student";
    public const string Parent  = "Parent";

    /// <summary>Sistemde tanımlı tüm roller.</summary>
    public static readonly IReadOnlyList<string> All = new[] { Admin, Teacher, Student, Parent };

    /// <summary>Geçerli bir rol adı mı (büyük/küçük harf duyarlı — Identity NormalizeName kullanır).</summary>
    public static bool IsValid(string? roleName)
        => !string.IsNullOrWhiteSpace(roleName) && All.Contains(roleName);
}
