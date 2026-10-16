namespace Kela.Core.Modules.Nodes;

/// <summary>Video konversiya çıxışlarının bucket açarları.</summary>
public static class NodePaths
{
    private const string RawMarker = "/videos/raw/";

    /// <summary>
    /// Çıxış prefiksi mənbə açarından törədilir ki, fayl hansı tenant qovluğundadırsa
    /// çıxış da orada olsun (tenant adı sonradan dəyişsə belə uyğun qalsın).
    /// </summary>
    public static string OutputPrefix(string sourcePath, long videoId, int version, string tenantFolder)
    {
        var index = sourcePath.IndexOf(RawMarker, StringComparison.OrdinalIgnoreCase);
        var baseFolder = index > 0 ? sourcePath[..index] : tenantFolder;

        return $"{baseFolder.TrimEnd('/')}/videos/m3u8/{videoId}/v{version}";
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
}
