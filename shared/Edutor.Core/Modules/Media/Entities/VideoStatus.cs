namespace Edutor.Core.Modules.Media.Entities;

/// <summary>
/// Video konversiyasının vəziyyəti. Yükləmə bitəndə "pending" olur; xarici worker
/// m3u8 + thumbnail hazırlayıb callback ucu ilə "ready" (və ya "failed") edir.
/// </summary>
public static class VideoStatus
{
    /// <summary>Yüklənib, konversiya gözləyir.</summary>
    public const string Pending = "pending";

    /// <summary>Worker götürüb, hazırlanır.</summary>
    public const string Processing = "processing";

    /// <summary>m3u8 hazırdır — app onu oynadır.</summary>
    public const string Ready = "ready";

    /// <summary>Konversiya alınmadı.</summary>
    public const string Failed = "failed";
}
