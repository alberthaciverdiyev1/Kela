using System.Text;

namespace Edutor.Core.Common.Storage;

/// <summary>Bucket açarları üçün fayl adlarının təmizlənməsi.</summary>
public static class StorageNaming
{
    /// <summary>
    /// İstifadəçinin yüklədiyi adı saxlayır, yalnız təhlükəli simvolları əvəz edir:
    /// qovluq ayırıcıları atılır, boşluq və xüsusi simvollar "-" olur.
    /// </summary>
    public static string Sanitize(string? fileName)
    {
        var name = Path.GetFileName(fileName ?? string.Empty);
        if (string.IsNullOrWhiteSpace(name)) return "file";

        var extension = Path.GetExtension(name).ToLowerInvariant();
        var stem = Path.GetFileNameWithoutExtension(name);

        var sb = new StringBuilder(stem.Length);
        foreach (var ch in stem)
        {
            sb.Append(char.IsLetterOrDigit(ch) || ch is '-' or '_' or '.' ? ch : '-');
        }

        var result = sb.ToString();
        while (result.Contains("--")) result = result.Replace("--", "-");
        result = result.Trim('-', '.');
        if (result.Length == 0) result = "file";
        if (result.Length > 80) result = result[..80];

        return result + extension;
    }
}
