using Microsoft.AspNetCore.Http;

namespace Kela.Core.Modules.Homework.Services;

/// <summary>Tələbə faylları üçün vahid limitlər — həm servis, həm validator istifadə edir.</summary>
public static class HomeworkFileRules
{
    public const int MaxFiles = 5;
    public const long MaxFileBytes = 10 * 1024 * 1024;
    public const long MaxRequestBytes = 64 * 1024 * 1024;

    public static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx", ".txt", ".jpg", ".jpeg", ".png", ".zip"
    };

    public static void Validate(IReadOnlyList<IFormFile> files)
    {
        if (files.Count > MaxFiles) throw new ArgumentException($"At most {MaxFiles} files are allowed");
        if (files.Any(f => f.Length == 0)) throw new ArgumentException("Empty files are not allowed");
        if (files.Any(f => f.Length > MaxFileBytes)) throw new ArgumentException("Each file must be at most 10 MB");

        foreach (var file in files)
        {
            var extension = Path.GetExtension(file.FileName);
            if (!AllowedExtensions.Contains(extension))
            {
                throw new ArgumentException($"File type is not allowed: {extension}");
            }
        }
    }

    public static bool IsHttpUrl(string value)
        => Uri.TryCreate(value, UriKind.Absolute, out var uri)
           && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
