using System.Text;

namespace Edutor.Core.Modules.Nodes;

/// <summary>Video konversiya çıxışlarının bucket açarları.</summary>
public static class NodePaths
{
    private const string RawMarker = "/videos/raw/";

    /// <summary>
    /// Çıxış prefiksi mənbə açarından törədilir ki, fayl hansı tenant qovluğundadırsa
    /// çıxış da orada olsun (tenant adı sonradan dəyişsə belə uyğun qalsın).
    ///
    /// Qovluq adı orijinal fayl adıdır (məs. <c>content-video</c>) — bucket-da
    /// insan tərəfindən tanınsın. Mənbə açarı dəyişməz olduğu və yükləmə zamanı
    /// təkrarlar <c>-1</c>, <c>-2</c> ilə fərqləndirildiyi üçün bu ad stabildir.
    /// </summary>
    public static string OutputPrefix(string sourcePath, long videoId, string? title, int version, string tenantFolder)
    {
        var index = sourcePath.IndexOf(RawMarker, StringComparison.OrdinalIgnoreCase);
        var baseFolder = index > 0 ? sourcePath[..index] : tenantFolder;

        // Əvvəl orijinal fayl adı, yoxdursa video başlığı, o da yoxdursa ID.
        var folder = Slug(Path.GetFileNameWithoutExtension(sourcePath));
        if (folder.Length == 0) folder = Slug(title);
        if (folder.Length == 0) folder = $"video-{videoId}";

        return $"{baseFolder.TrimEnd('/')}/videos/m3u8/{folder}/v{version}";
    }

    public static string MasterKey(string outputPrefix) => $"{outputPrefix}/master.m3u8";

    public static string VariantKey(string outputPrefix, string resolution) => $"{resolution}/index.m3u8";

    public static string VariantFolder(string outputPrefix, string resolution) => $"{outputPrefix}/{resolution}";

    public static string ThumbnailKey(string outputPrefix) => $"{outputPrefix}/thumb.jpg";

    /// <summary>
    /// Worker-dan gələn nisbi açarı təhlükəsiz şəkildə prefiksə birləşdirir.
    /// Qovluqdan çıxma (".."), mütləq yol və sxem rədd edilir.
    /// </summary>
    public static string? Combine(string outputPrefix, string relativeKey)
    {
        if (string.IsNullOrWhiteSpace(relativeKey)) return null;

        var key = relativeKey.Replace('\\', '/').Trim();
        if (key.StartsWith('/')) key = key.TrimStart('/');

        if (key.Length == 0
            || key.Contains("..", StringComparison.Ordinal)
            || key.Contains("://", StringComparison.Ordinal)
            || key.Contains('\0')
            || key.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || key.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return $"{outputPrefix.TrimEnd('/')}/{key}";
    }

    /// <summary>Doğrulama üçün: açar prefiksin altındadırmı və təhlükəsizdirmi?</summary>
    public static bool IsInsidePrefix(string prefix, string key)
        => key.StartsWith(prefix.TrimEnd('/') + "/", StringComparison.Ordinal)
           && !key.Contains("..", StringComparison.Ordinal);

    /// <summary>Adı bucket açarına yararlı hala salır (kiçik hərf, ASCII, tire).</summary>
    private static string Slug(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        var builder = new StringBuilder(value.Length);
        var lastWasDash = false;

        foreach (var ch in value.Trim().ToLowerInvariant())
        {
            // Azərbaycan hərfləri əvvəlcə sadələşdirilir ki, "ə" → "e" olsun.
            var mapped = ch switch
            {
                'ə' => 'e', 'ı' => 'i', 'ö' => 'o', 'ü' => 'u', 'ç' => 'c',
                'ş' => 's', 'ğ' => 'g', 'ñ' => 'n', 'ё' => 'e',
                _ => ch
            };

            if (mapped is >= 'a' and <= 'z' or >= '0' and <= '9')
            {
                builder.Append(mapped);
                lastWasDash = false;
            }
            else if (!lastWasDash && builder.Length > 0)
            {
                builder.Append('-');
                lastWasDash = true;
            }
        }

        var result = builder.ToString().Trim('-');

        return result.Length <= 60 ? result : result[..60].Trim('-');
    }
}
